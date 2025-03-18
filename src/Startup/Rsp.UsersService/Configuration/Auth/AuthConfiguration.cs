using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.FeatureManagement;
using Microsoft.Net.Http.Headers;
using Rsp.Logging.Extensions;
using Rsp.UsersService.Application.Authentication.Helpers;
using Rsp.UsersService.Application.Constants;
using Rsp.UsersService.Application.Settings;

namespace Rsp.UsersService.Configuration.Auth;

/// <summary>
/// Authentication and Authorization configuration
/// </summary>
[ExcludeFromCodeCoverage]
public static class AuthConfiguration
{
    /// <summary>
    /// Adds the Authentication and Authorization to the service
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <param name="appSettings">Application Settinghs</param>
    public static IServiceCollection AddAuthenticationAndAuthorization(this IServiceCollection services, AppSettings appSettings)
    {
        ConfigureJwt(services, appSettings);

        ConfigureAuthorization(services);

        return services;
    }

    private static void ConfigureJwt(IServiceCollection services, AppSettings appSettings)
    {
        var events = new JwtBearerEvents
        {
            OnMessageReceived = async context =>
            {
                var tokenHelper = context.Request.HttpContext.RequestServices.GetRequiredService<ITokenHelper>();
                var authorization = context.Request.Headers[HeaderNames.Authorization];

                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                // If no authorization header found, nothing to process further
                if (string.IsNullOrWhiteSpace(authorization))
                {
                    logger.LogAsWarning("Authorization header is empty");

                    context.NoResult();
                    return;
                }

                // if authorization starts with "Bearer " replace that with empty string
                context.Token = tokenHelper.DeBearerizeAuthToken(authorization);

                var featureManager = context.HttpContext.RequestServices.GetRequiredService<IFeatureManager>();

                if (await featureManager.IsEnabledAsync(Features.AuthTokenLogging))
                {
                    logger.LogAsWarning($"AuthToken: {context.Token}", "Auth Token logging is enabled");
                }
            },
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                logger.LogAsWarning("Authentication Failed");

                context.Fail(context.Exception);

                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                logger.LogAsInformation("AuthToken Validated");

                return Task.CompletedTask;
            }
        };

        // Enable built-in authentication of Jwt bearer token
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            // using the scheme JwtBearerDefaults.AuthenticationScheme (Bearer)
            .AddJwtBearer(authOptions => JwtBearerConfiguration.Configure(authOptions, appSettings, events));
    }

    private static void ConfigureAuthorization(IServiceCollection services)
    {
        // amend the default policy so that
        // it checks for email and role claim
        // in addition to just an authenticated user
        var defaultPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireClaim(ClaimTypes.Email)
            .RequireClaim(ClaimTypes.Role)
            .RequireRole("admin", "iras_portal_user")
            .Build();

        // set the default policy for [Authorize] attribute
        // without a policy name
        services
            .AddAuthorizationBuilder()
            .SetDefaultPolicy(defaultPolicy);

        // add an authorization handler to handle the requirements e.g. for a user in a
        // particular role. The requirement can be linked directly to the the custom [Authorize]
        // attribute or to the policy, which you can specify in [Authorize(Name = policyName)].
        // services.AddSingleton<IAuthorizationHandler, RequirementHandler>()
    }
}