namespace Rsp.UsersService.WebApi.Requests;

public class UserRegisterRequest
{
    public string Title { get; set; } = null!;

    /// <summary>
    /// The user's given name.
    /// </summary>
    public string GivenName { get; set; } = null!;

    /// <summary>
    /// The user's family name.
    /// </summary>
    public string FamilyName { get; set; } = null!;

    /// <summary>
    /// The user's email address which acts as a user name.
    /// </summary>
    public string Email { get; set; } = null!;

    public string? Country { get; set; } = null;
    public string? JobTitle { get; set; } = null;
    public string? Telephone { get; set; } = null;
    public string? Organisation { get; set; } = null;
    public string? Role { get; set; } = null;
    public string Status { get; set; } = null!;
    public DateTime? LastUpdated { get; set; } = null;
    public DateTime? CurrentLogin { get; set; } = null;
}