using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.WebApi.Requests;
using static Rsp.UsersService.WebApi.Helpers.ValidationProblemHelpers;

namespace Rsp.UsersService.WebApi.Endpoints.Users;

public static class AddUserClaimsEndpoint
{
    public static async Task<Results<NotFound<string>, ValidationProblem, NoContent>> AddUserClaims<TUser>
            ([FromServices] IServiceProvider sp, [FromBody] UserClaimsRequest claimsRequest) where TUser : IrasUser, new()
    {
        var userManager = sp.GetRequiredService<UserManager<TUser>>();

        var user = await userManager.FindByEmailAsync(claimsRequest.Email);

        if (user == null)
        {
            return TypedResults.NotFound($"User with email {claimsRequest.Email} not found");
        }

        var result = await userManager.AddClaimsAsync(user, claimsRequest.Claims.ConvertAll(c => new Claim(c.Key, c.Value)));

        if (!result.Succeeded)
        {
            return CreateValidationProblem(result);
        }

        return TypedResults.NoContent();
    }
}