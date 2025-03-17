using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.WebApi.DTOs;
using Rsp.UsersService.WebApi.Responses;

namespace Rsp.UsersService.WebApi.Endpoints.Users;

public static class GetUsersInRoleEndpoint
{
    public static async Task<Results<NotFound<string>, Ok<AllUsersResponse>>> GetUsersInRole<TUser>
    (
        [FromServices] IServiceProvider sp,
        string roleName
    ) where TUser : IrasUser, new()
    {
        var userManager = sp.GetRequiredService<UserManager<TUser>>();

        if (await userManager.GetUsersInRoleAsync(roleName) is not IList<TUser> users)
        {
            return TypedResults.NotFound($"No users were found in {roleName} role");
        }

        return TypedResults.Ok
         (
             new AllUsersResponse
             {
                 Users = users.Select
                 (
                     user => new UserDto
                     (
                        user.Id,
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
                        user.LastUpdated
                     )
                 ),
                 TotalCount = await userManager.Users.CountAsync()
             }
         );
    }
}