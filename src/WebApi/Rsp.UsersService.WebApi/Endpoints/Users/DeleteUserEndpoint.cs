using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Rsp.UsersService.Domain.Entities;
using static Rsp.UsersService.WebApi.Helpers.ValidationProblemHelpers;

namespace Rsp.UsersService.WebApi.Endpoints.Users;

public static class DeleteUserEndpoint
{
    public static async Task<Results<ValidationProblem, NotFound<string>, NoContent>> DeleteUser<TUser>
    (
        [FromServices] IServiceProvider sp,
        string? id,
        string? email
    ) where TUser : IrasUser, new()
    {
        var userManager = sp.GetRequiredService<UserManager<TUser>>();

        if (id == null && email == null)
        {
            return CreateValidationProblem("Missing_Parameters", "Please provide id or email to search for the user");
        }

        var user = (id, email) switch
        {
            { id: null, email: not null } => await userManager.FindByEmailAsync(email),
            { id: not null, email: null } => await userManager.FindByIdAsync(id),
            { id: not null, email: not null } => await userManager.FindByIdAsync(id),
            _ => null
        };

        if (user == null)
        {
            var errorMessage = (id, email) switch
            {
                { id: null, email: not null } => $"User with email {email} not found",
                { id: not null, email: null } => $"User with id {id} not found",
                { id: not null, email: not null } => $"User with id {id} or email {email} not found",
                _ => "User not found"
            };

            return TypedResults.NotFound(errorMessage);
        }

        var result = await userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            return CreateValidationProblem(result);
        }

        return TypedResults.NoContent();
    }
}