using Microsoft.AspNetCore.Identity;

namespace Rsp.UsersService.Infrastructure.SeedData;

internal static class UserData
{
    public static IList<IdentityRole> SeedRoles()
    {
        return SeedHelper.SeedData<IdentityRole>("Roles.json");
    }
}