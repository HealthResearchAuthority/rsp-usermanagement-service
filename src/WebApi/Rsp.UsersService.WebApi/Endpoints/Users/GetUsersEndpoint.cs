using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.WebApi.DTOs;
using Rsp.UsersService.WebApi.Responses;
using static Rsp.UsersService.WebApi.Helpers.ValidationProblemHelpers;

namespace Rsp.UsersService.WebApi.Endpoints.Users;

public static class GetUsersEndpoint
{
    public static async Task<Results<ValidationProblem, NotFound<string>, Ok<AllUsersResponse>>> GetUsersById<TUser>
    (
        [FromServices] IServiceProvider sp,
        [FromBody] IEnumerable<string> ids,
        string? searchQuery = null,
        int pageIndex = 1,
        int pageSize = 10
    ) where TUser : IrasUser, new()
    {
        if (ids == null)
        {
            return CreateValidationProblem("Missing_Parameters", "Please provide list of ids to search for the user");
        }

        var userManager = sp.GetRequiredService<UserManager<TUser>>();

        var baseQuery = userManager
                .Users
                .Where(x => ids.Contains(x.Id));
        if (!string.IsNullOrEmpty(searchQuery))
        {
            var splitQuery = searchQuery.Split(' ');

            // apply search term if available
            baseQuery = baseQuery.
                Where(x =>
                        splitQuery.All(word =>
                        x.FirstName.Contains(word)
                    || x.LastName.Contains(word)
                    || x.Email!.Contains(word)
                 ));
        }

        var users = await baseQuery
                .OrderBy(x => x.FirstName)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        if (users == null)
        {
            return TypedResults.NotFound("Users not found");
        }

        var usersCount = await baseQuery.CountAsync();

        return TypedResults.Ok
       (
           new AllUsersResponse
           {
               Users = users.Select
               (
                   user => user.Adapt<UserDto>()
               ),
               TotalCount = usersCount
           }
       );
    }
}