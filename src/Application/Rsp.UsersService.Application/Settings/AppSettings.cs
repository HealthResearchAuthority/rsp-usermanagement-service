namespace Rsp.UsersService.Application.Settings;

public class AppSettings
{
    /// <summary>
    /// Label to use when reading App Configuration from AzureAppConfiguration
    /// </summary>
    public const string ServiceLabel = "usersservice";

    /// <summary>
    /// Gets or sets authentication settings
    /// </summary>
    public AuthSettings AuthSettings { get; set; } = null!;

    public AzureAppConfigurations AzureAppConfiguration { get; set; } = null!;
}