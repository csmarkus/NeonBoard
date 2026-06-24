using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NeonBoard.Api;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Domain.Users;
using NeonBoard.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace NeonBoard.IntegrationTests.Infrastructure;

public class NeonBoardWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:16-alpine")
        .Build();

    public Guid TestUserId { get; private set; }

    private TestCurrentUserService _testCurrentUserService = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();

            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                var interceptors = serviceProvider
                    .GetServices<Microsoft.EntityFrameworkCore.Diagnostics.ISaveChangesInterceptor>();

                options.UseNpgsql(_dbContainer.GetConnectionString())
                    .AddInterceptors(interceptors);
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.AuthenticationScheme, _ => { });

            services.RemoveAll<ICurrentUserService>();
            services.AddScoped<ICurrentUserService>(_ => _testCurrentUserService);
        });

        builder.UseEnvironment("Development");
    }

    public async ValueTask InitializeAsync()
    {
        await _dbContainer.StartAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = User.Create(
            TestAuthHandler.TestAuth0UserId,
            TestAuthHandler.TestEmail,
            TestAuthHandler.TestName);

        TestUserId = user.Id;
        _testCurrentUserService.SetUserId(TestUserId);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
    }

    public new async ValueTask DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }
    
    public async Task<T> CreateProjectAsync<T>(HttpClient client, string name = "Test Project", string description = "Test Description")
    {
        var response = await client.PostAsJsonAsync("/api/projects", new { Name = name, Description = description });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }

    public async Task<T> CreateBoardAsync<T>(HttpClient client, Guid projectId, string name = "Test Board")
    {
        var response = await client.PostAsJsonAsync($"/api/projects/{projectId}/boards", new { Name = name });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }

    public async Task<T> AddColumnAsync<T>(HttpClient client, Guid projectId, Guid boardId, string name = "Test Column")
    {
        var response = await client.PostAsJsonAsync(
            $"/api/projects/{projectId}/boards/{boardId}/columns",
            new { Name = name });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }

    public async Task<T> AddCardAsync<T>(HttpClient client, Guid projectId, Guid boardId, Guid columnId, string title = "Test Card", string? description = null)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/projects/{projectId}/boards/{boardId}/cards",
            new { ColumnId = columnId, Title = title, Description = description ?? "Test description" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }

    public async Task<T> AddLabelAsync<T>(HttpClient client, Guid projectId, Guid boardId, string name = "Bug", string color = "red")
    {
        var response = await client.PostAsJsonAsync(
            $"/api/projects/{projectId}/boards/{boardId}/labels",
            new { Name = name, Color = color });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }
}
