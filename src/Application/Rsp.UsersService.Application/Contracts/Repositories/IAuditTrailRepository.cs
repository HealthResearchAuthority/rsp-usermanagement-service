using Rsp.UsersService.Domain.Entities;

namespace Rsp.UsersService.Application;

public interface IAuditTrailRepository
{
    public Task<IEnumerable<UserAuditTrail>> GetUserAuditTrail(string userId);
}