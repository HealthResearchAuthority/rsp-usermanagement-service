using Microsoft.EntityFrameworkCore.ChangeTracking;
using Rsp.UsersService.Domain.Entities;

namespace Rsp.UsersService.Infrastructure.Helpers;

public interface IAuditTrailHandler
{
    public bool CanHandle(object entity);

    public Task<IEnumerable<UserAuditTrail>> GenerateAuditTrails(EntityEntry entity, IrasUser? systemAdmin, IrasIdentityDbContext? context = null);
}