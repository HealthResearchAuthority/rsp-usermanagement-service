using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.Domain.Interfaces;
using Rsp.UsersService.Infrastructure.Helpers;

namespace Rsp.UsersService.Infrastructure.Interceptors;

[ExcludeFromCodeCoverage]
public class AuditTrailInterceptor(IAuditTrailDetailsService auditTrailDetailsService, IEnumerable<IAuditTrailHandler> auditTrailHandlers) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync
    (
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.Context is not IrasIdentityDbContext dbContext)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var auditableEntries = eventData
            .Context
            .ChangeTracker
            .Entries<IAuditable>();

        if (!auditableEntries.Any())
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var systemAdmin = await dbContext
            .Users
            .FirstOrDefaultAsync(u => u.Email == auditTrailDetailsService.GetEmailFromHttpContext(), cancellationToken);

        var auditTrailRecords = new List<UserAuditTrail>();

        foreach (var entry in auditableEntries)
        {
            foreach (var auditTrailHandler in auditTrailHandlers)
            {
                if (auditTrailHandler.CanHandle(entry.Entity))
                {
                    auditTrailRecords.AddRange(await auditTrailHandler.GenerateAuditTrails(entry, systemAdmin!, dbContext));
                }
            }
        }

        await dbContext.UserAuditTrails.AddRangeAsync(auditTrailRecords, cancellationToken);

        return await new ValueTask<InterceptionResult<int>>(result);
    }
}