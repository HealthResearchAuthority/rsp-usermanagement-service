using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Rsp.UsersService.WebApi.DTOs;

namespace Rsp.UsersService.WebApi.Endpoints.Roles;

public static class GetRoleClaimsEndpoint
{
    public static async Task<Results<NotFound<string>, Ok<IEnumerable<ClaimDto>>>> GetRoleClaims<TRole>
    (
        [FromServices] IServiceProvider sp,
        string roleName
    ) where TRole : IdentityRole, new()
    {
        var roleManager = sp.GetRequiredService<RoleManager<TRole>>();

        var role = roleManager.Roles.FirstOrDefault(r => r.Name == roleName);

        if (role == null)
        {
            return TypedResults.NotFound($"{roleName} not found");
        }

        var claims = await roleManager.GetClaimsAsync(role);

        return TypedResults.Ok
        (
            claims.Select(claim => new ClaimDto(claim.Type, claim.Value))
        );
    }
}