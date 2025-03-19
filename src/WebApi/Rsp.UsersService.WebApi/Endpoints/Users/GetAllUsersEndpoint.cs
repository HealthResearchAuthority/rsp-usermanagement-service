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

public static class GetAllUsersEndpoint
{
    public static async Task<Results<BadRequest<string>, Ok<AllUsersResponse>>> GetAllUsers<TUser>
    (
        [FromServices] IServiceProvider sp,
        int pageIndex = 1,
        int pageSize = 10
    ) where TUser : IrasUser, new()
    {
        if (pageIndex < 1 || pageSize < 1)
        {
            return TypedResults.BadRequest("PageIndex and PageSize should be greater than 0");
        }

        var userManager = sp.GetRequiredService<UserManager<TUser>>();

        var users = await userManager
            .Users
            .OrderBy(u => u.FirstName)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

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