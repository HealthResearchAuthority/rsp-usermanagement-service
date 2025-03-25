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

    /// <summary>
    /// Gets or sets Azure App Configuration settings
    /// </summary>
    public AzureAppConfigurations AzureAppConfiguration { get; set; } = null!;

    /// <summary>
    /// Gets or sets OneLogin settings
    /// </summary>
    public OneLoginConfiguration OneLogin { get; set; } = null!;
}