using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.FeatureManagement;
using Microsoft.Net.Http.Headers;
using Rsp.Logging.Extensions;
using Rsp.UsersService.Application.Authentication.Helpers;
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
    /// <param name="config"><see cref="IConfiguration"/></param>
    public static IServiceCollection AddAuthenticationAndAuthorization(this IServiceCollection services, AppSettings appSettings, IConfiguration config)
    {
        ConfigureJwt(services, appSettings, config);

        ConfigureAuthorization(services);

        return services;
    }

    private static void ConfigureJwt(IServiceCollection services, AppSettings appSettings, IConfiguration config)
    {
        var events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var tokenHelper = context.Request.HttpContext.RequestServices.GetRequiredService<ITokenHelper>();
                var authorization = context.Request.Headers[HeaderNames.Authorization];

                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                // If no authorization header found, nothing to process further
                if (string.IsNullOrWhiteSpace(authorization))
                {
                    logger.LogAsWarning("Authorization header is empty");

                    context.NoResult();
                    return Task.CompletedTask;
                }

                // if authorization starts with "Bearer " replace that with empty string
                context.Token = tokenHelper.DeBearerizeAuthToken(authorization);

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                logger.LogAsWarning("Authentication Failed");
                logger.LogAsError("ERR_API_AUTH_FAILED", "API Authetication failed", context.Exception);

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

        var featureManager = new FeatureManager(new ConfigurationFeatureDefinitionProvider(config));

        // Enable built-in authentication of Jwt bearer token
        services
            .AddAuthentication()
            // using the scheme JwtBearerDefaults.AuthenticationScheme (Bearer)
            .AddJwtBearer("DefaultBearer", async authOptions => await JwtBearerConfiguration.Configure(authOptions, appSettings, events, featureManager))
            .AddJwtBearer("FunctionAppBearer", options =>
            {
                options.Authority = appSettings.MicrosoftEntra.Authority;
                options.Audience = appSettings.MicrosoftEntra.Audience;
                options.Events = events;
            })
            .AddPolicyScheme("dynamicBearer", null, options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    var tokenHelper = context.Request.HttpContext.RequestServices.GetRequiredService<ITokenHelper>();
                    var authToken = context.Request.Headers[HeaderNames.Authorization];

                    // if we don't have token, there is nothing to forward to
                    if (string.IsNullOrWhiteSpace(authToken))
                    {
                        return JwtBearerDefaults.AuthenticationScheme;
                    }

                    // replace the "Bearer " if present in the token
                    var token = tokenHelper.DeBearerizeAuthToken(authToken);
                    var jwtHandler = new JwtSecurityTokenHandler();

                    // if we can't read the token, return the empty scheme
                    if (!jwtHandler.CanReadToken(token))
                    {
                        return JwtBearerDefaults.AuthenticationScheme;
                    }

                    // get the token to verify the issuer
                    var jwtSecurityToken = jwtHandler.ReadJwtToken(token);

                    // based on the issuer, we will forward the request to the appropriate scheme
                    // if the token issuer is the one for OneLogin, use the default JwtBearer scheme
                    // if the issuer is the one for Microsoft Entra ID, use the FunctionAppBearer scheme
                    return jwtSecurityToken.Issuer == appSettings.MicrosoftEntra.Authority ? "FunctionAppBearer" : "DefaultBearer";
                };
            });
    }

    private static void ConfigureAuthorization(IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Policy that only uses defaultBearer scheme
            options.AddPolicy("UseDefaultBearerOnly", policy =>
            {
                policy
                    .AddAuthenticationSchemes("defaultBearer")
                    .RequireAuthenticatedUser()
                    .RequireClaim(ClaimTypes.Email)
                    .RequireClaim(ClaimTypes.Role);
            });

            // Optional: default policy can use dynamicBearer if you want
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes("dynamicBearer")
                .RequireAuthenticatedUser()
                .Build();
        });
    }
}