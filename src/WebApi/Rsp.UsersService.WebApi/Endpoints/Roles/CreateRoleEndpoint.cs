using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using static Rsp.UsersService.WebApi.Helpers.ValidationProblemHelpers;

namespace Rsp.UsersService.WebApi.Endpoints.Roles;

public static class CreateRoleEndpoint
{
    public static async Task<Results<ValidationProblem, NoContent>> CreateRole<TRole>
    (
        [FromServices] IServiceProvider sp,
        string roleName
    ) where TRole : IdentityRole, new()
    {
        var roleManager = sp.GetRequiredService<RoleManager<TRole>>();

        var result = await roleManager.CreateAsync(new TRole { Name = roleName });

        if (result.Errors.Any())
        {
            return CreateValidationProblem(result);
        }

        return TypedResults.NoContent();
    }
}