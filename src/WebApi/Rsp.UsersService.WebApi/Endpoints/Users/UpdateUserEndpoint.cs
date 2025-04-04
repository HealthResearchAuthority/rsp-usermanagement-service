﻿using System.ComponentModel.DataAnnotations;
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
using Rsp.UsersService.WebApi.Requests;
using static Rsp.UsersService.WebApi.Helpers.ValidationProblemHelpers;

namespace Rsp.UsersService.WebApi.Endpoints.Users;

[ExcludeFromCodeCoverage]
public static class UpdateUserEndpoint
{
    // Validate the email address using DataAnnotations like the UserValidator does when RequireUniqueEmail = true.
    private static readonly EmailAddressAttribute _emailAddressAttribute = new();

    // NOTE: We cannot inject UserManager<TUser> directly because the TUser generic parameter is currently unsupported by RDG.
    // https://github.com/dotnet/aspnetcore/issues/47338

    public static async Task<Results<ValidationProblem, NotFound<string>, NoContent>> UpdateUser<TUser>
    (
        string email,
        [FromBody] UserRegisterRequest newUserDetails,
        [FromServices] IServiceProvider sp,
        HttpContext httpContext
    ) where TUser : IrasUser, new()
    {
        var userManager = sp.GetRequiredService<UserManager<TUser>>();

        var userStore = sp.GetRequiredService<IUserStore<TUser>>();
        var emailStore = (IUserEmailStore<TUser>)userStore;

        if (string.IsNullOrEmpty(newUserDetails.Email) || !_emailAddressAttribute.IsValid(newUserDetails.Email))
        {
            return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(newUserDetails.Email)));
        }

        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return TypedResults.NotFound($"{email} not found");
        }

        // Create deep copy of user to retain values when user is changed
        var oldUser = new TUser();
        foreach (var prop in typeof(TUser).GetProperties())
        {
            if (prop.CanRead && prop.CanWrite)
            {
                prop.SetValue(oldUser, prop.GetValue(user));
            }
        }

        user.FirstName = newUserDetails.FirstName;
        user.LastName = newUserDetails.LastName;
        user.Title = newUserDetails.Title;
        user.Telephone = newUserDetails.Telephone;
        user.Organisation = newUserDetails.Organisation;
        user.Country = newUserDetails.Country;
        user.JobTitle = newUserDetails.JobTitle;
        user.LastUpdated = newUserDetails.LastUpdated;
        user.Status = newUserDetails.Status;

        await userStore.SetUserNameAsync(user, newUserDetails.Email, CancellationToken.None);
        await emailStore.SetEmailAsync(user, newUserDetails.Email, CancellationToken.None);
        var result = await userManager.UpdateAsync(user);

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
                AuditTrailActions.Update,
                sysAdmin!.Id,
                oldUser
            );

            var auditRespository = sp.GetRequiredService<IAuditTrailRepository>();

            await auditRespository.CreateAuditRecords(auditTrail);
        }

        return TypedResults.NoContent();
    }
}