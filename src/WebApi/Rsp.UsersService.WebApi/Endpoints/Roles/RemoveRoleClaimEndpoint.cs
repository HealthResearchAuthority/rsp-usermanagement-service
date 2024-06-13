using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Rsp.UsersService.WebApi.Requests;
using static Rsp.UsersService.WebApi.Helpers.ValidationProblemHelpers;

namespace Rsp.UsersService.WebApi.Endpoints.Roles;

public static class RemoveRoleClaimEndpoint
{
    public static async Task<Results<NotFound<string>, ValidationProblem, NoContent>> RemoveRoleClaim<TRole>
    (
        [FromServices] IServiceProvider sp,
        [FromBody] RoleClaimRequest claimRequest
    ) where TRole : IdentityRole, new()
    {
        var roleManager = sp.GetRequiredService<RoleManager<TRole>>();

        if (roleManager.Roles.FirstOrDefault(r => r.Name == claimRequest.Role) is not TRole role)
        {
            return TypedResults.NotFound($"{claimRequest.Role} not found");
        }

        var roleClaims = await roleManager.GetClaimsAsync(role);

        var claim = roleClaims.FirstOrDefault(c => c.Type == claimRequest.ClaimType && c.Value == claimRequest.ClaimValue);

        if (claim == null)
        {
            return TypedResults.NotFound($"Claim with type {claimRequest.ClaimType} and value {claimRequest.ClaimValue} not found");
        }

        var result = await roleManager.RemoveClaimAsync(role, claim);

        if (result.Errors.Any())
        {
            return CreateValidationProblem(result);
        }

        return TypedResults.NoContent();
    }
}