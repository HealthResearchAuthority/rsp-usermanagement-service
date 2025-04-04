using Microsoft.AspNetCore.Identity;
using Rsp.UsersService.Domain.Entities;

namespace Rsp.UsersService.WebApi.Endpoints.Users;

public static class UserHelper
{
    public static async Task<TUser?> FindUserAsync<TUser>(UserManager<TUser> userManager, string? id, string? email) where TUser : IrasUser
    {
        if (!string.IsNullOrEmpty(email))
        {
            return await userManager.FindByEmailAsync(email);
        }

        if (!string.IsNullOrEmpty(id))
        {
            return await userManager.FindByIdAsync(id);
        }
        return null;
    }

    public static string GetNotFoundMessage(string? id, string? email)
    {
        return id switch
        {
            not null when email is null => $"User with id {id} not found",
            null when email is not null => $"User with email {email} not found",
            not null when email is not null => $"User with id {id} or email {email} not found",
            _ => "User not found"
        };
    }
}