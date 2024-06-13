namespace Rsp.UsersService.WebApi.Requests;

public class RoleClaimRequest
{
    public string Role { get; set; } = null!;

    public string ClaimType { get; set; } = null!;

    public string ClaimValue { get; set; } = null!;
}