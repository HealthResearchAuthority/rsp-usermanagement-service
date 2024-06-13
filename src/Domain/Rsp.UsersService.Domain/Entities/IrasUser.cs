using Microsoft.AspNetCore.Identity;

namespace Rsp.UsersService.Domain.Entities;

public class IrasUser : IdentityUser
{
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;
}