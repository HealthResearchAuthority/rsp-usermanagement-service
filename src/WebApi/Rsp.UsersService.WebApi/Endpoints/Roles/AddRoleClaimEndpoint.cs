using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Rsp.UsersService.WebApi.Requests;
using static Rsp.UsersService.WebApi.Helpers.ValidationProblemHelpers;

namespace Rsp.UsersService.WebApi.Endpoints.Roles;

public static class AddRoleClaimEndpoint
{
    public static async Task<Results<NotFound<string>, ValidationProblem, NoContent>> AddRoleClaim<TRole>
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
        var claim = new Claim(claimRequest.ClaimType, claimRequest.ClaimValue);

        var result = await roleManager.AddClaimAsync(role, claim);

        if (result.Errors.Any())
        {
            return CreateValidationProblem(result);
        }

        return TypedResults.NoContent();
    }
}