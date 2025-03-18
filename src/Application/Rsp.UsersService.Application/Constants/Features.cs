namespace Rsp.UsersService.Application.Constants;

/// <summary>
/// Defines constants for feature names.
/// </summary>
public static class Features
{
    // Intercepts the start/end of method calls if enabled
    public const string InterceptedLogging = "Logging.InterceptedLogging";

    // Logs the AuthToken if enabled
    public const string AuthTokenLogging = "Logging.AuthTokenLogging";
}