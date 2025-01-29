using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.WebApi.DTOs;
using static Rsp.UsersService.WebApi.Helpers.ValidationProblemHelpers;

namespace Rsp.UsersService.WebApi.Endpoints.Users;

public static class GetUserClaimsEndpoint
{
    public static async Task<Results<ValidationProblem, NotFound<string>, Ok<IEnumerable<ClaimDto>>>> GetUserClaims<TUser>
    (
        [FromServices] IServiceProvider sp,
        string? id,
        string? email
    ) where TUser : IrasUser, new()
    {
        if (id == null && email == null)
        {
            return CreateValidationProblem("Missing_Parameters", "Please provide id or email to search for the user");
        }

        var userManager = sp.GetRequiredService<UserManager<TUser>>();
        var user = await UserHelper.FindUserAsync(userManager, id, email);

        if (user == null)
        {
            return TypedResults.NotFound(UserHelper.GetNotFoundMessage(id, email));
        }

        var claims = await userManager.GetClaimsAsync(user);

        return TypedResults.Ok(claims.Select(claim => new ClaimDto(claim.Type, claim.Value)));
    }
}