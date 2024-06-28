namespace Rsp.UsersService.WebApi.Requests;

public class UserRegisterRequest
{
    /// <summary>
    /// The user's first name.
    /// </summary>
    public string FirstName { get; init; } = null!;

    /// <summary>
    /// The user's last name.
    /// </summary>
    public string LastName { get; init; } = null!;

    /// <summary>
    /// The user's email address which acts as a user name.
    /// </summary>
    public string Email { get; init; } = null!;
}