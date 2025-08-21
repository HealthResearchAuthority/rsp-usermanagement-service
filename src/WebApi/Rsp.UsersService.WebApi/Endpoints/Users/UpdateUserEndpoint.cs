using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.WebApi.Requests;
using static Rsp.UsersService.WebApi.Helpers.ValidationProblemHelpers;

namespace Rsp.UsersService.WebApi.Endpoints.Users;

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
        [FromServices] IServiceProvider sp
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

        user.GivenName = newUserDetails.GivenName;
        user.FamilyName = newUserDetails.FamilyName;
        user.Title = newUserDetails.Title;
        user.Telephone = newUserDetails.Telephone;
        user.Organisation = newUserDetails.Organisation;
        user.Country = newUserDetails.Country;
        user.JobTitle = newUserDetails.JobTitle;
        user.LastUpdated = newUserDetails.LastUpdated;
        user.Status = newUserDetails.Status;
        user.IdentityProviderId = newUserDetails.IdentityProviderId;

        if (newUserDetails.CurrentLogin != null)
        {
            user.LastLogin = user.CurrentLogin;
            user.CurrentLogin = newUserDetails.CurrentLogin;
        }

        await userStore.SetUserNameAsync(user, newUserDetails.Email, CancellationToken.None);
        await emailStore.SetEmailAsync(user, newUserDetails.Email, CancellationToken.None);
        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return CreateValidationProblem(result);
        }

        return TypedResults.NoContent();
    }
}