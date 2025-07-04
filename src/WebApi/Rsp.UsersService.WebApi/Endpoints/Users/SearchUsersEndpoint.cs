﻿using Mapster;
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

public static class SearchUsersEndpoint
{
    public static async Task<Results<ValidationProblem, NotFound<string>, Ok<AllUsersResponse>>> SearchUsers<TUser>
    (
        [FromServices] IServiceProvider sp,
        string searchQuery,
        [FromBody] IEnumerable<string>? userIdsToIgnore = null,
        int pageIndex = 1,
        int pageSize = 10
    ) where TUser : IrasUser, new()
    {
        if (string.IsNullOrEmpty(searchQuery))
        {
            return CreateValidationProblem("Missing_Parameters", "Please provide a search query");
        }

        if (pageIndex < 1 || pageSize < 1)
        {
            return CreateValidationProblem("Bad_Request", "PageIndex and PageSize should be greater than 0");
        }

        userIdsToIgnore ??= new List<string>();
        var userManager = sp.GetRequiredService<UserManager<TUser>>();

        var splitQuery = searchQuery.Split(' ');

        var users = await userManager
                .Users
                .Where(u => !userIdsToIgnore.Contains(u.Id))
                .Where(x =>
                    splitQuery.All(w =>
                        x.GivenName.Contains(w)
                        || x.FamilyName.Contains(w)
                        || x.Email!.Contains(w)
                        ))
                .OrderBy(x => x.GivenName)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        if (users == null)
        {
            return TypedResults.NotFound("Users not found");
        }

        var userCount = await userManager
                .Users
                .Where(u => !userIdsToIgnore.Contains(u.Id))
                .Where(x =>
                    x.GivenName.Contains(searchQuery)
                    || x.FamilyName.Contains(searchQuery)
                    || x.Email!.Contains(searchQuery))
                .CountAsync();

        return TypedResults.Ok
       (
           new AllUsersResponse
           {
               Users = users.Select
               (
                   user => user.Adapt<UserDto>()
               ),
               TotalCount = userCount
           }
       );
    }
}