using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Rsp.UsersService.Application.Constants;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.WebApi.DTOs;
using Rsp.UsersService.WebApi.Responses;
using static Rsp.UsersService.WebApi.Helpers.ValidationProblemHelpers;

namespace Rsp.UsersService.WebApi.Endpoints.Users;

public static class GetUserEndpoint
{
    public static async Task<Results<ValidationProblem, NotFound<string>, Ok<UserResponse>>> GetUserByIdOrEmail<TUser>
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

        var roles = await userManager.GetRolesAsync(user);

        var userClaims = await userManager.GetClaimsAsync(user);
        var accessRequired = userClaims.Where(x => x.Type == UserClaimTypes.AccessRequired).Select(x => x.Value);

        return TypedResults.Ok(new UserResponse
        {
            User = new UserDto(user.Id,
            user.FirstName,
            user.LastName,
            user.Email!,
            user.Title,
            user.JobTitle,
            user.Organisation,
            user.Telephone,
            user.Country,
            user.Status,
            user.LastLogin,
            user.LastUpdated),
            Roles = roles.AsEnumerable(),
            AccessRequired = accessRequired.AsEnumerable(),
        });
    }
}