using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Rsp.UsersService.Application;
using Rsp.UsersService.Application.Constants;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.WebApi.Helpers;
using static Rsp.UsersService.WebApi.Helpers.ValidationProblemHelpers;

namespace Rsp.UsersService.WebApi.Endpoints.Users;

[ExcludeFromCodeCoverage]
public static class AddUserToRolesEndpoint
{
    public static async Task<Results<NotFound<string>, ValidationProblem, NoContent>> AddUserToRoles<TUser>
    (
        [FromServices] IServiceProvider sp,
        HttpContext httpContext,
        string email,
        string roles
    ) where TUser : IrasUser, new()
    {
        var userManager = sp.GetRequiredService<UserManager<TUser>>();

        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return TypedResults.NotFound($"User with email {email} not found");
        }

        var userRoles = await userManager.GetRolesAsync(user);

        var rolesToAdd = roles
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Where(role => !userRoles.Contains(role));

        if (rolesToAdd.Any())
        {
            var result = await userManager.AddToRolesAsync(user, rolesToAdd);

            if (!result.Succeeded)
            {
                return CreateValidationProblem(result);
            }

            var sysAdmin = await UserHelper.FindUserAsync
            (
                userManager,
                null,
                UserHelper.GetAuthenticatedUserEmail(httpContext.User)!
            );

            if (sysAdmin != null)
            {
                var auditTrail = AuditTrailHelper.GenerateAuditTrail
                (
                    user,
                    AuditTrailActions.AddRole,
                    sysAdmin!.Id,
                    roles: rolesToAdd
                );

                var auditRespository = sp.GetRequiredService<IAuditTrailRepository>();

                await auditRespository.CreateAuditRecords(auditTrail);
            }
        }

        return TypedResults.NoContent();
    }
}