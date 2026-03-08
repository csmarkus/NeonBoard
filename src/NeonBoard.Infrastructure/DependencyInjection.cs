using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NeonBoard.Application.Common.Interfaces;
using NeonBoard.Infrastructure.Persistence;
using NeonBoard.Infrastructure.Persistence.Interceptors;
using NeonBoard.Infrastructure.Repositories;
using NeonBoard.Infrastructure.Services;

namespace NeonBoard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<ISaveChangesInterceptor, DomainEventDispatcherInterceptor>();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var interceptors = serviceProvider.GetServices<ISaveChangesInterceptor>();

            options.UseNpgsql(configuration.GetConnectionString("neonboarddb"))
                .AddInterceptors(interceptors);
        });

        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IBoardRepository, BoardRepository>();
        services.AddScoped<IActivityEntryRepository, ActivityEntryRepository>();
        services.AddScoped<IProjectInvitationRepository, ProjectInvitationRepository>();
        services.AddScoped<IProjectAuthorizationService, ProjectAuthorizationService>();
        services.AddScoped<IEmailService, ConsoleEmailService>();

        return services;
    }
}
