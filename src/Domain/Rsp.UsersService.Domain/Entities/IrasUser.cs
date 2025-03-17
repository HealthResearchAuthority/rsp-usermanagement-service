using Microsoft.AspNetCore.Identity;

namespace Rsp.UsersService.Domain.Entities;

public class IrasUser : IdentityUser
{
    public string? Title { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Telephone { get; set; } = null;
    public string? Organisation { get; set; } = null;
    public string? Country { get; set; } = null;
    public string? JobTitle { get; set; } = null;
    public string Status { get; set; } = null!;
    public DateTime? LastUpdated { get; set; } = null;
    public DateTime? LastLogin { get; set; } = null;
}