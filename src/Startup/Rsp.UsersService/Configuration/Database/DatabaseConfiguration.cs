using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Rsp.UsersService.Infrastructure;
using Rsp.UsersService.Infrastructure.Interceptors;

namespace Rsp.UsersService.Configuration.Database;

/// <summary>
/// Adds DbContext to the application
/// </summary>
public static class DatabaseConfiguration
{
    /// <summary>
    /// Adds services to the IoC container
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <param name="configuration"><see cref="IConfiguration"/></param>
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IrasIdentityDbContext>((serviceProvider, options) =>
            options
                .UseSqlServer(configuration.GetConnectionString("IdentityDbConnection"))
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
                .AddInterceptors(serviceProvider.GetRequiredService<AuditTrailInterceptor>())
        );

        return services;
    }
}