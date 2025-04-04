using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rsp.UsersService.Domain.Entities;

namespace Rsp.UsersService.Infrastructure.EntitiesConfiguration;

public class UserAuditTrailConfiguration : IEntityTypeConfiguration<UserAuditTrail>
{
    public void Configure(EntityTypeBuilder<UserAuditTrail> builder)
    {
        builder
            .HasKey(at => at.Id);

        builder
            .HasOne(at => at.SystemAdministrator)
            .WithMany()
            .HasForeignKey(at => at.SystemAdministratorId)
            .HasPrincipalKey(u => u.Id)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne(at => at.User)
            .WithMany()
            .HasForeignKey(at => at.UserId)
            .HasPrincipalKey(u => u.Id)
            .OnDelete(DeleteBehavior.NoAction);
    }
}