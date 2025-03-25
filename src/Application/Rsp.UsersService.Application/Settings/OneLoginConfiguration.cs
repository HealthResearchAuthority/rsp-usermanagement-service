namespace Rsp.UsersService.Application.Settings;

public class OneLoginConfiguration
{
    /// <summary>
    /// One Login Token Issuers
    /// </summary>
    public List<string> Issuers { get; set; } = null!;

    /// <summary>
    /// The client identifier.
    /// </summary>
    public string ClientId { get; set; } = null!;
}