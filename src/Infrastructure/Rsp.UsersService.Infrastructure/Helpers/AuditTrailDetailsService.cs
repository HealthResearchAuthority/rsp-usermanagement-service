using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Rsp.UsersService.Infrastructure.Helpers;

public class AuditTrailDetailsService(IHttpContextAccessor contextAccessor) : IAuditTrailDetailsService
{
    public string GetEmailFromHttpContext()
    {
        var claimsPrinciple = contextAccessor?.HttpContext?.User;

        return claimsPrinciple!.Claims!.FirstOrDefault(x => x.Type == ClaimTypes.Email)!.Value!;
    }
}