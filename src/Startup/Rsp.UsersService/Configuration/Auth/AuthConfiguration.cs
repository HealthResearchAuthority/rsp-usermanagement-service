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
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                var authorization = context.Request.Headers[HeaderNames.Authorization].ToString();

                logger.LogInformation("Authorization header: " + authorization);

                if (string.IsNullOrWhiteSpace(authorization))
                {
                    logger.LogWarning("Authorization header is empty");
                    context.NoResult();
                    return Task.CompletedTask;
                }

                var token = tokenHelper.DeBearerizeAuthToken(authorization);

                logger.LogInformation("Extracted token: " + (token?.Length > 20 ? token.Substring(0, 20) : token));

                context.Token = token;

                return Task.CompletedTask;
            },

            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                logger.LogWarning("Authentication failed");
                logger.LogError("Authentication exception: " + context.Exception);

                return Task.CompletedTask;
            },

            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                var claims = context.Principal?.Claims
                    .Select(c => c.Type + ": " + c.Value);

                logger.LogInformation("Token validated. Claims: " + string.Join(", ", claims ?? []));

                return Task.CompletedTask;
            }
        };

        var featureManager = new FeatureManager(new ConfigurationFeatureDefinitionProvider(config));

        services
            .AddAuthentication()
            .AddJwtBearer("DefaultBearer", async authOptions =>
                await JwtBearerConfiguration.Configure(authOptions, appSettings, events, featureManager))

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
                    var logger = context.Request.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    var tokenHelper = context.Request.HttpContext.RequestServices.GetRequiredService<ITokenHelper>();

                    var authHeader = context.Request.Headers[HeaderNames.Authorization].ToString();

                    logger.LogInformation("Forward selector - header: " + authHeader);

                    if (string.IsNullOrWhiteSpace(authHeader))
                    {
                        logger.LogWarning("No auth header, using default scheme");
                        return JwtBearerDefaults.AuthenticationScheme;
                    }

                    var token = tokenHelper.DeBearerizeAuthToken(authHeader);

                    logger.LogInformation("Token for scheme selection: " + (token?.Length > 20 ? token.Substring(0, 20) : token));

                    var jwtHandler = new JwtSecurityTokenHandler();

                    if (!jwtHandler.CanReadToken(token))
                    {
                        logger.LogWarning("Cannot read token, using default scheme");
                        return JwtBearerDefaults.AuthenticationScheme;
                    }

                    var jwtToken = jwtHandler.ReadJwtToken(token);

                    logger.LogInformation("Token issuer: " + jwtToken.Issuer);
                    logger.LogInformation("Expected authority: " + appSettings.MicrosoftEntra.Authority);

                    var scheme = jwtToken.Issuer == appSettings.MicrosoftEntra.Authority
                        ? "FunctionAppBearer"
                        : "DefaultBearer";

                    logger.LogInformation("Selected scheme: " + scheme);

                    return scheme;
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