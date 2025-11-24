using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Rsp.UsersService.Domain.Entities;

namespace Rsp.UsersService.Infrastructure.Helpers;

public class UserRoleAuditTrailHandler : IAuditTrailHandler
{
    public bool CanHandle(object entity) => entity is UserRole;

    public async Task<IEnumerable<UserAuditTrail>> GenerateAuditTrails(EntityEntry entity, IrasUser systemAdmin, IrasIdentityDbContext? context = null)
    {
        if (entity.Entity is not UserRole userRole || context == null)
        {
            return [];
        }

        var auditTrailRecords = new List<UserAuditTrail>();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userRole.UserId);
        var role = await context.Roles.FirstOrDefaultAsync(r => r.Id == userRole.RoleId);

        switch (entity.State)
        {
            case EntityState.Added:
                var addAuditTrail = new UserAuditTrail()
                {
                    DateTimeStamp = DateTime.UtcNow,
                    Description = $"{user!.Email} was assigned {role!.Name!.Replace('_', ' ')} role",
                    UserId = user!.Id,
                    SystemAdministratorId = systemAdmin.Id
                };

                auditTrailRecords.Add(addAuditTrail);
                break;

            case EntityState.Deleted:
                var deleteAuditTrail = new UserAuditTrail()
                {
                    DateTimeStamp = DateTime.UtcNow,
                    Description = $"{user!.Email} was unassigned {role!.Name!.Replace('_', ' ')} role",
                    UserId = user!.Id,
                    SystemAdministratorId = systemAdmin.Id
                };

                auditTrailRecords.Add(deleteAuditTrail);
                break;
        }

        return auditTrailRecords;
    }
}