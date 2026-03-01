using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using NeonBoard.Api.Configuration;
using NeonBoard.Api.Endpoints;
using NeonBoard.Api.Middleware;
using NeonBoard.Api.Services;
using NeonBoard.Application;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Infrastructure;
using NeonBoard.Infrastructure.Persistence;
using Serilog;

namespace NeonBoard.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/neonboard-.log",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{CorrelationId}] {SourceContext} {Message:lj}{NewLine}{Exception}"));

        builder.AddServiceDefaults();

        var auth0Domain = builder.Configuration["Auth0:Domain"];
        var auth0Audience = builder.Configuration["Auth0:Audience"];

        if (string.IsNullOrEmpty(auth0Domain) || string.IsNullOrEmpty(auth0Audience))
        {
            throw new InvalidOperationException(
                "Auth0 configuration is missing. Please set Auth0:Domain and Auth0:Audience in appsettings.json");
        }

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.Authority = $"https://{auth0Domain}/";
            options.Audience = auth0Audience;
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true
            };
        });
        builder.Services.AddAuthorization();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);

        string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? (builder.Environment.IsDevelopment()
              ? ["http://localhost:4200", "https://localhost:4200"]
              : []);

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        var rateLimitSettings = builder.Configuration
            .GetSection(RateLimitSettings.SectionName)
            .Get<RateLimitSettings>() ?? new RateLimitSettings();

        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                var retryAfter = context.Lease.TryGetMetadata(
                    MetadataName.RetryAfter, out var retryAfterValue)
                    ? (int)retryAfterValue.TotalSeconds
                    : rateLimitSettings.WindowInSeconds;

                context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();

                var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Too Many Requests",
                    Type = "https://httpstatuses.com/429",
                    Detail = $"Rate limit exceeded. Try again in {retryAfter} seconds."
                };

                await context.HttpContext.Response.WriteAsJsonAsync(
                    problemDetails,
                    (System.Text.Json.JsonSerializerOptions?)null,
                    "application/problem+json",
                    cancellationToken);
            };

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? context.User?.FindFirst("sub")?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    return RateLimitPartition.GetFixedWindowLimiter(
                        $"user_{userId}",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = rateLimitSettings.AuthenticatedPermitLimit,
                            Window = TimeSpan.FromSeconds(rateLimitSettings.WindowInSeconds),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        });
                }

                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(
                    $"ip_{ipAddress}",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitSettings.AnonymousPermitLimit,
                        Window = TimeSpan.FromSeconds(rateLimitSettings.WindowInSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });
        });

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Apply database migrations on startup
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation("Applying database migrations...");
                dbContext.Database.Migrate();
                logger.LogInformation("Database migrations applied successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating the database");
                throw;
            }
        }

        app.UseExceptionHandler();
        app.UseMiddleware<CorrelationIdMiddleware>();

        app.UseCors();

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseAuthentication();
        app.UseRateLimiter();
        app.UseAuthorization();

        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.MapDefaultEndpoints();
        app.MapProjectEndpoints();
        app.MapBoardEndpoints();
        app.MapColumnEndpoints();
        app.MapCardEndpoints();
        app.MapLabelEndpoints();

        app.MapFallbackToFile("index.html");

        app.Run();
    }
}
