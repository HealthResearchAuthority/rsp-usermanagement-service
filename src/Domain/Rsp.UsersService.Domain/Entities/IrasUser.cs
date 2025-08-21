using Microsoft.AspNetCore.Identity;
using Rsp.UsersService.Domain.Attributes;
using Rsp.UsersService.Domain.Interfaces;

namespace Rsp.UsersService.Domain.Entities;

public class IrasUser : IdentityUser, IAuditable
{
    public string? IdentityProviderId { get; set; }

    [Auditable]
    public string? Title { get; set; } = null!;

    [Auditable]
    public string? FamilyName { get; set; }

    [Auditable]
    public string? GivenName { get; set; }

    [Auditable]
    public override string Email { get; set; } = null!;

    [Auditable]
    public string? Telephone { get; set; } = null;

    [Auditable]
    public string? Organisation { get; set; } = null!;

    [Auditable]
    public string? Country { get; set; } = null;

    [Auditable]
    public string? JobTitle { get; set; } = null;

    [Auditable]
    public string Status { get; set; } = null!;

    public DateTime? LastUpdated { get; set; } = null;
    public DateTime? LastLogin { get; set; } = null;
    public DateTime? CurrentLogin { get; set; } = null;
}