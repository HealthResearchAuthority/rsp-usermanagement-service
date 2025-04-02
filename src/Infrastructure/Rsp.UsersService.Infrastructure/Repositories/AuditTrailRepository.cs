using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;
using Rsp.UsersService.Application;
using Rsp.UsersService.Domain.Entities;

namespace Rsp.UsersService.Infrastructure.Repositories;

public class AuditTrailRepository(IrasIdentityDbContext context) : IAuditTrailRepository
{
    public async Task<IEnumerable<UserAuditTrail>> GetUserAuditTrail(string userId)
    {
        return await context.UserAuditTrails
            .Include(at => at.User)
            .Include(at => at.SystemAdministrator)
            .Where(at => at.UserId == userId)
            .ToListAsync();
    }

    public async Task CreateAuditRecords(IEnumerable<UserAuditTrail> userAuditTrails)
    {
        await context.UserAuditTrails.AddRangeAsync(userAuditTrails);
        await context.SaveChangesAsync();
    }

    public async Task<int> GetAuditTrailCount(string userId)
    {
        return await context
            .UserAuditTrails
            .Where(at => at.UserId == userId)
            .CountAsync();
    }
}