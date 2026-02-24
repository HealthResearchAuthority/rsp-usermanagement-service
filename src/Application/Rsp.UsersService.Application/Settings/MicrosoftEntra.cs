namespace Rsp.UsersService.Application.Settings;

public class MicrosoftEntra
{
    /// <summary>
    /// Authrity URL for the Microsoft Entra tenant
    /// </summary>
    public string Authority { get; set; } = null!;

    /// <summary>
    /// The API Client ID for the IRAS Service from Microsoft Entra
    /// </summary>
    public string Audience { get; set; } = null!;
}