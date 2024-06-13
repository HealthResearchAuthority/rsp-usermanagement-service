using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

namespace Rsp.UsersService.Application.Authentication.Helpers;

public class TokenHelper : ITokenHelper
{
    /// <summary>
    /// Removes the word "Bearer " if present
    /// </summary>
    /// <param name="authToken">The authentication token.</param>
    public string DeBearerizeAuthToken(StringValues authToken)
    {
        if (authToken.Count == 0 || authToken[0] == null)
        {
            throw new SecurityTokenException("Empty authorization token");
        }

        var token = authToken[0];

        // to decode the token, we need to replace the "Bearer " with empty string
        // otherwise, JwtSecurityToken initialization will throw an error
        if (token!.StartsWith(JwtBearerDefaults.AuthenticationScheme, StringComparison.OrdinalIgnoreCase))
        {
            token = token
                .Replace(JwtBearerDefaults.AuthenticationScheme, string.Empty)
                .TrimStart(' ');
        }

        return token;
    }

    /// <summary>
    /// Adds "Bearer " as a prefix if not present
    /// </summary>
    /// <param name="authToken">The authentication token.</param>
    public string BearerizeAuthToken(StringValues authToken)
    {
        if (authToken.Count == 0 || authToken[0] == null)
        {
            throw new SecurityTokenException("Empty authorization token");
        }

        var token = authToken[0];

        // to decode the token, we need to replace the "Bearer " with empty string
        // otherwise, JwtSecurityToken initialization will throw an error
        if (!token!.StartsWith(JwtBearerDefaults.AuthenticationScheme, StringComparison.OrdinalIgnoreCase))
        {
            return "Bearer " + token.Trim();
        }

        return token;
    }
}