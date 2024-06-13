using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using static Rsp.UsersService.WebApi.Helpers.ValidationProblemHelpers;

namespace Rsp.UsersService.WebApi.Endpoints.Roles;

public static class DeleteRoleEndpoint
{
    public static async Task<Results<NotFound<string>, ValidationProblem, NoContent>> DeleteRole<TRole>
    (
        [FromServices] IServiceProvider sp,
        string roleName
    ) where TRole : IdentityRole, new()
    {
        var roleManager = sp.GetRequiredService<RoleManager<TRole>>();
        var role = await roleManager.FindByNameAsync(roleName);

        if (role == null)
        {
            return TypedResults.NotFound($"{roleName} not found");
        }

        var result = await roleManager.DeleteAsync(role);

        if (result.Errors.Any())
        {
            return CreateValidationProblem(result);
        }

        return TypedResults.NoContent();
    }
}