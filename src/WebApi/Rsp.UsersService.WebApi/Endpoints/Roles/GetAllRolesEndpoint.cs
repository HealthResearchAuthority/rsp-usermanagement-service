using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rsp.UsersService.WebApi.DTOs;
using Rsp.UsersService.WebApi.Responses;

namespace Rsp.UsersService.WebApi.Endpoints.Roles;

public static class GetAllRolesEndpoint
{
    public static async Task<Results<BadRequest<string>, Ok<AllRolesResponse>>> GetAllRoles<TRole>
    (
        [FromServices] IServiceProvider sp,
        int pageIndex = 1,
        int pageSize = 10
    ) where TRole : IdentityRole, new()
    {
        if (pageIndex < 1 || pageSize < 1)
        {
            return TypedResults.BadRequest("PageIndex and PageSize should be greater than 0");
        }

        var roleManager = sp.GetRequiredService<RoleManager<TRole>>();

        var roles = await roleManager
            .Roles
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return TypedResults.Ok(new AllRolesResponse
        {
            Roles = roles.Select(role => new RoleDto(role.Id, role.Name!)),
            TotalCount = await roleManager
                    .Roles
                    .CountAsync()
        });
    }
}