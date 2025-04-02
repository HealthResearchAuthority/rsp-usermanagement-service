using Rsp.UsersService.Domain.Entities;

namespace Rsp.UsersService.Application;

public interface IAuditTrailRepository
{
    public Task<IEnumerable<UserAuditTrail>> GetUserAuditTrail(string userId);

    public Task CreateAuditRecords(IEnumerable<UserAuditTrail> userAuditTrails);

    public Task<int> GetAuditTrailCount(string userId);
}