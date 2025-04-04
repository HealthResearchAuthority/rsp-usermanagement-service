using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Rsp.UsersService.Application;
using Rsp.UsersService.WebApi.DTOs;
using Rsp.UsersService.WebApi.Responses;

namespace Rsp.UsersService.WebApi.Endpoints.Users;

public static class GetUserAuditTrailEndpoint
{
    public static async Task<Results<NotFound<string>, Ok<UserAuditTrailResponse>>> GetUserAuditTrail
    (
        [FromServices] IServiceProvider sp,
        string userId
    )
    {
        var respository = sp.GetRequiredService<IAuditTrailRepository>();

        var userAuditTrail = await respository.GetUserAuditTrail(userId);

        if (!userAuditTrail.Any())
        {
            return TypedResults.NotFound($"No audit trail found for userId = {userId}");
        }

        return TypedResults.Ok
        (
           new UserAuditTrailResponse
           {
               Name = $"{userAuditTrail.First().User.FirstName} {userAuditTrail.First().User.LastName}",
               Items = [.. userAuditTrail.Select
               (
                   at => new UserAuditTrailDto {
                       SystemAdmin = at.SystemAdministrator.Email!,
                       DateTimeStamp = at.DateTimeStamp,
                       Description = at.Description
                   }
               ).OrderByDescending(item => item.DateTimeStamp)]
           }
        );
    }
}