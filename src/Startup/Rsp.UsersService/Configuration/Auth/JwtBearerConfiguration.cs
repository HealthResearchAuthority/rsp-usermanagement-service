using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.FeatureManagement;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.JwtExtensions;
using Rsp.UsersService.Application.Constants;
using Rsp.UsersService.Application.Settings;

namespace Rsp.UsersService.Configuration.Auth;

/// <summary>
/// JwtBearer Configuration
/// </summary>
[ExcludeFromCodeCoverage]
public static class JwtBearerConfiguration
{
    /// <summary>
    /// Jwt Bearer Configuration
    /// </summary>
    /// <param name="authOptions"><see cref="JwtBearerOptions"/></param>
    /// <param name="appSettings">Application Settings</param>
    /// <param name="jwtBearerEvents"><see cref="JwtBearerEvents"/></param>
    /// <param name="featureManager"><see cref="IFeatureManager"/></param>
    public static async Task Configure(JwtBearerOptions authOptions, AppSettings appSettings, JwtBearerEvents jwtBearerEvents, IFeatureManager featureManager)
    {
        authOptions.SetJwksOptions(new JwkOptions(appSettings.AuthSettings.JwksUri));

        // Set a valid audience value for any received OpenIdConnect token.
        // This value is passed into TokenValidationParameters.ValidAudience if that property is empty.
        // Alternatively, set the value below in TokenValidationParameters.ValidAudience
        // or TokenValidationParameters.ValidAudiences (if more than one audience)

        // check if Gov UK One Login integration is enabled
        var oneLoginEnabled = await featureManager.IsEnabledAsync(Features.OneLogin);

        authOptions.Audience = oneLoginEnabled ? appSettings.OneLogin.ClientId : appSettings.AuthSettings.ClientId;
        authOptions.RequireHttpsMetadata = true;
        authOptions.SaveToken = true;

        // Specify token validation parameters.
        // Note: TokenValidationParameters.ClockSkew has default value of 300 seconds (5 minutes) which can be changed by setting ClockSkew below.
        authOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuers = oneLoginEnabled ? appSettings.OneLogin.Issuers : appSettings.AuthSettings.Issuers,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true
        };

        // Add event handlers
        authOptions.Events = jwtBearerEvents;
    }
}