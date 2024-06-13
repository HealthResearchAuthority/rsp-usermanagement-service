using Rsp.UsersService.Application.Authentication.Helpers;

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
        // services.AddMediatR(option => option.RegisterServicesFromAssemblyContaining<IApplication>())

        return services;
    }
}