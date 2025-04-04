using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Rsp.UsersService.Domain.Entities;
using static Rsp.UsersService.WebApi.Helpers.ValidationProblemHelpers;

namespace Rsp.UsersService.WebApi.Endpoints.Users;

public static class AddUserToRolesEndpoint
{
    public static async Task<Results<NotFound<string>, ValidationProblem, NoContent>> AddUserToRoles<TUser>
    (
        [FromServices] IServiceProvider sp,
        string email,
        string roles
    ) where TUser : IrasUser, new()
    {
        var userManager = sp.GetRequiredService<UserManager<TUser>>();

        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return TypedResults.NotFound($"User with email {email} not found");
        }

        var userRoles = await userManager.GetRolesAsync(user);

        var rolesToAdd = roles
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Where(role => !userRoles.Contains(role));

        if (rolesToAdd.Any())
        {
            var result = await userManager.AddToRolesAsync(user, rolesToAdd);

            if (!result.Succeeded)
            {
                return CreateValidationProblem(result);
            }
        }

        return TypedResults.NoContent();
    }
}