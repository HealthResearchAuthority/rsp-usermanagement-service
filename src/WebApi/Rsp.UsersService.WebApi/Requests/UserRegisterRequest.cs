namespace Rsp.UsersService.WebApi.Requests;

public class UserRegisterRequest
{
    /// <summary>
    /// The user's first name.
    /// </summary>
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// The user's last name.
    /// </summary>
    public string LastName { get; set; } = null!;

    /// <summary>
    /// The user's email address which acts as a user name.
    /// </summary>
    public string Email { get; set; } = null!;
}