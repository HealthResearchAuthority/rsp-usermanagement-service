using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rsp.UsersService.Application.Constants;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.WebApi.DTOs;
using Rsp.UsersService.WebApi.Requests;
using Rsp.UsersService.WebApi.Responses;

namespace Rsp.UsersService.WebApi.Endpoints.Users;

public static class GetAllUsersEndpoint
{
    public static async Task<Results<BadRequest<string>, Ok<AllUsersResponse>>> GetAllUsers<TUser>
    (
        [FromServices] IServiceProvider sp,
        [FromBody] SearchUserRequest? searchQuery = null,
        int pageIndex = 1,
        int pageSize = 10,
        string sortField = nameof(UserDto.GivenName),
        string sortDirection =  SortDirections.Descending
    ) where TUser : IrasUser, new()
    {
        if (pageIndex < 1 || pageSize < 1)
        {
            return TypedResults.BadRequest("PageIndex and PageSize should be greater than 0");
        }

        var userManager = sp.GetRequiredService<UserManager<TUser>>();

        var baseQuery = userManager.Users;

        if (searchQuery != null)
        {
            if (!string.IsNullOrEmpty(searchQuery.SearchQuery))
            {
                var splitQuery = searchQuery.SearchQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                baseQuery = baseQuery.Where(x =>
                    splitQuery.Any(word =>
                        x.GivenName.ToLower().Contains(word.ToLower()) ||
                        x.FamilyName.ToLower().Contains(word.ToLower()) ||
                        (x.Email != null && x.Email.ToLower().Contains(word.ToLower()))
                    ));
            }

            if (searchQuery.Status.HasValue)
            {
                var statusText = searchQuery.Status.Value ? "Active" : "Disabled";
                baseQuery = baseQuery.Where(x => x.Status == statusText);
            }

            if (searchQuery.FromDate.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.LastLogin >= searchQuery.FromDate.Value);
            }

            if (searchQuery.ToDate.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.LastLogin <= searchQuery.ToDate.Value);
            }
        }

        // Apply sorting
        baseQuery = ApplyOrdering(baseQuery, sortField, sortDirection);

        // Materialize result (EF-safe filters only so far)
        var usersList = await baseQuery.ToListAsync();

        // ✅ Now filter by Country in memory
        if (searchQuery?.Country is { Count: > 0 })
        {
            var lowerCountries = searchQuery.Country
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.ToLowerInvariant())
                .ToList();

            usersList = usersList
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.Country) &&
                    x.Country
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(c => c.Trim().ToLowerInvariant())
                        .Intersect(lowerCountries)
                        .Any())
                .ToList();
        }

        // ✅ Now paginate in memory after full filtering
        var usersCount = usersList.Count;

        var users 
            = usersList
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

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

    /// <summary>
    /// Dynamically applies ordering to a queryable based on field and direction.
    /// </summary>
    private static IQueryable<TUser> ApplyOrdering<TUser>(IQueryable<TUser> query, string sortField, string sortDirection) where TUser : IrasUser
    {
        var field = sortField.ToLowerInvariant();

        return (field, sortDirection) switch
        {
            ("givenname", SortDirections.Ascending) => query
                .OrderBy(x => x.GivenName)
                .ThenByDescending(x => x.CurrentLogin)
                .ThenBy(x => x.Status),

            ("givenname", SortDirections.Descending) => query
                .OrderByDescending(x => x.GivenName)
                .ThenByDescending(x => x.CurrentLogin)
                .ThenBy(x => x.Status),

            ("familyname", SortDirections.Ascending) => query
                .OrderBy(x => x.FamilyName)
                .ThenByDescending(x => x.CurrentLogin)
                .ThenBy(x => x.Status),

            ("familyname", SortDirections.Descending) => query
                .OrderByDescending(x => x.FamilyName)
                .ThenByDescending(x => x.CurrentLogin)
                .ThenBy(x => x.Status),

            ("email", SortDirections.Ascending) => query
                .OrderBy(x => x.Email)
                .ThenByDescending(x => x.CurrentLogin)
                .ThenBy(x => x.Status),

            ("email", SortDirections.Descending) => query
                .OrderByDescending(x => x.Email)
                .ThenByDescending(x => x.CurrentLogin)
                .ThenBy(x => x.Status),

            ("status", SortDirections.Ascending) => query
                .OrderBy(x => x.Status)
                .ThenByDescending(x => x.CurrentLogin),

            ("status", SortDirections.Descending) => query
                .OrderByDescending(x => x.Status)
                .ThenByDescending(x => x.CurrentLogin)
                .ThenBy(x => x.Status),

            ("currentlogin", SortDirections.Ascending) => query
                .OrderBy(x => x.CurrentLogin)
                .ThenBy(x => x.Status),

            ("currentlogin", SortDirections.Descending) => query
                .OrderByDescending(x => x.CurrentLogin)
                .ThenBy(x => x.Status),

            _ => query
                .OrderBy(x => x.GivenName)
                .ThenByDescending(x => x.CurrentLogin)
                .ThenBy(x => x.Status)
        };
    }
}