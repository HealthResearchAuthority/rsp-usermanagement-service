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
        string? searchQuery = null,
        int pageIndex = 1,
        int pageSize = 10
    ) where TUser : IrasUser, new()
    {
        if (pageIndex < 1 || pageSize < 1)
        {
            return TypedResults.BadRequest("PageIndex and PageSize should be greater than 0");
        }

        var userManager = sp.GetRequiredService<UserManager<TUser>>();

        var baseQuery = userManager.Users;

        if (!string.IsNullOrEmpty(searchQuery))
        {
            var splitQuery = searchQuery.Split(' ');

            // apply search term if available
            baseQuery = baseQuery.
                Where(x =>
                        splitQuery.All(word =>
                        x.GivenName.Contains(word)
                    || x.FamilyName.Contains(word)
                    || x.Email!.Contains(word)
                 ));
        }

        var usersCount = await baseQuery.CountAsync();

        var users = await baseQuery
            .OrderBy(u => u.GivenName)
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
                        user.GivenName,
                        user.FamilyName,
                        user.Email!,
                        user.Title,
                        user.JobTitle,
                        user.Organisation,
                        user.Telephone,
                        user.Country,
                        user.Status,
                        user.LastLogin,
                        user.CurrentLogin,
                        user.LastUpdated
                    )
                ),
                TotalCount = usersCount
            }
        );
    }
}