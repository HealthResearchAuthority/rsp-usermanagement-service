using Rsp.UsersService.Application;
using Rsp.UsersService.Application.Authentication.Helpers;
using Rsp.UsersService.Infrastructure.Helpers;
using Rsp.UsersService.Infrastructure.Interceptors;
using Rsp.UsersService.Infrastructure.Repositories;

namespace Rsp.UsersService.Configuration.Dependencies;

/// <summary>
///  User Defined Services Configuration
/// </summary>
public static class ServicesConfiguration
{
    /// <summary>
    /// Adds services to the IoC container
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        // example of configuring the IoC container to inject the dependencies

        services.AddSingleton<ITokenHelper, TokenHelper>();
        services.AddTransient<IAuditTrailRepository, AuditTrailRepository>();
        services.AddTransient<IAuditTrailHandler, IrasUserAuditTrailHandler>();
        services.AddTransient<IAuditTrailHandler, UserRoleAuditTrailHandler>();
        services.AddTransient<AuditTrailInterceptor>();
        services.AddTransient<IAuditTrailDetailsService, AuditTrailDetailsService>();

        return services;
    }
}