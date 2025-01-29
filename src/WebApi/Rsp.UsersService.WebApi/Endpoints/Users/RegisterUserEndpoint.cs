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

public static class RegisterUserEndpoint
{
    // Validate the email address using DataAnnotations like the UserValidator does when RequireUniqueEmail = true.
    private static readonly EmailAddressAttribute _emailAddressAttribute = new();

    // NOTE: We cannot inject UserManager<TUser> directly because the TUser generic parameter is currently unsupported by RDG.
    // https://github.com/dotnet/aspnetcore/issues/47338

    public static async Task<Results<ValidationProblem, NoContent>> RegisterUser<TUser>
    (
        [FromBody] UserRegisterRequest registration,
        [FromServices] IServiceProvider sp
    ) where TUser : IrasUser, new()
    {
        var userManager = sp.GetRequiredService<UserManager<TUser>>();

        var email = registration.Email;

        if (string.IsNullOrEmpty(email) || !_emailAddressAttribute.IsValid(email))
        {
            return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(email)));
        }

        var userStore = sp.GetRequiredService<IUserStore<TUser>>();
        var emailStore = (IUserEmailStore<TUser>)userStore;

        var user = new TUser
        {
            FirstName = registration.FirstName,
            LastName = registration.LastName
        };

        await userStore.SetUserNameAsync(user, email, CancellationToken.None);
        await emailStore.SetEmailAsync(user, email, CancellationToken.None);
        var result = await userManager.CreateAsync(user);

        if (!result.Succeeded)
        {
            return CreateValidationProblem(result);
        }

        return TypedResults.NoContent();
    }
}