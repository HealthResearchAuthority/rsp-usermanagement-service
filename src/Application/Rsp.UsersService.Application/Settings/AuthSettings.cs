using System.Diagnostics.CodeAnalysis;

namespace Rsp.UsersService.Application.Settings;

// Modify as needed, these are typical properties for JWT authentication
// using Jwks, remove/modify as needed
[ExcludeFromCodeCoverage]
public class AuthSettings
{
    /// <summary>
    /// The value for token issuer / authority
    /// </summary>
    public string Authority { get; set; } = null!;

    /// <summary>
    /// The client identifier.
    /// </summary>
    public string ClientId { get; set; } = null!;

    /// <summary>
    /// JwksUri for verifying the jwt signature
    /// </summary>
    public string JwksUri { get; set; } = null!;
}