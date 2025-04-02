using System.ComponentModel.DataAnnotations;
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
public static class RegisterUserEndpoint
{
    // Validate the email address using DataAnnotations like the UserValidator does when RequireUniqueEmail = true.
    private static readonly EmailAddressAttribute _emailAddressAttribute = new();

    // NOTE: We cannot inject UserManager<TUser> directly because the TUser generic parameter is currently unsupported by RDG.
    // https://github.com/dotnet/aspnetcore/issues/47338

    public static async Task<Results<ValidationProblem, NoContent>> RegisterUser<TUser>
    (
        [FromBody] UserRegisterRequest registration,
        [FromServices] IServiceProvider sp,
        HttpContext httpContext
    ) where TUser : IrasUser, new()
    {
        var userManager = sp.GetRequiredService<UserManager<TUser>>();

        var email = registration.Email;

        if (string.IsNullOrEmpty(email) || !_emailAddressAttribute.IsValid(email))
        {
            return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(email)));
        }

        // check for existing user
        var duplicateUser = await userManager.FindByEmailAsync(email);
        if (duplicateUser != null)
        {
            return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.DuplicateEmail(email)));
        }

        var userStore = sp.GetRequiredService<IUserStore<TUser>>();
        var emailStore = (IUserEmailStore<TUser>)userStore;

        var user = new TUser
        {
            FirstName = registration.FirstName,
            LastName = registration.LastName,
            Telephone = registration.Telephone,
            Country = registration.Country,
            Title = registration.Title,
            JobTitle = registration.JobTitle,
            Organisation = registration.Organisation,
            Status = registration.Status,
            LastUpdated = registration.LastUpdated,
        };

        await userStore.SetUserNameAsync(user, email, CancellationToken.None);
        await emailStore.SetEmailAsync(user, email, CancellationToken.None);

        var result = await userManager.CreateAsync(user);

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
                AuditTrailActions.Create,
                sysAdmin!.Id
            );

            var auditRespository = sp.GetRequiredService<IAuditTrailRepository>();

            await auditRespository.CreateAuditRecords(auditTrail);
        }

        return TypedResults.NoContent();
    }
}