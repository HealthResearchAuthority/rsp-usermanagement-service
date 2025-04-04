using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.Infrastructure.EntitiesConfiguration;
using Rsp.UsersService.Infrastructure.SeedData;

namespace Rsp.UsersService.Infrastructure;

[ExcludeFromCodeCoverage]
public class IrasIdentityDbContext(DbContextOptions<IrasIdentityDbContext> options) :
    IdentityDbContext
    <
        IrasUser,
        IdentityRole,
        string,
        IdentityUserClaim<string>,
        UserRole,
        IdentityUserLogin<string>,
        IdentityRoleClaim<string>,
        IdentityUserToken<string>
    >(options)
{
    public DbSet<UserAuditTrail> UserAuditTrails { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IrasUser>(b =>
        {
            b.ToTable("Users");
            b.Ignore(u => u.AccessFailedCount);
            b.Ignore(u => u.EmailConfirmed);
            b.Ignore(u => u.LockoutEnabled);
            b.Ignore(u => u.LockoutEnd);
            b.Ignore(u => u.PasswordHash);
            b.Ignore(u => u.PhoneNumber);
            b.Ignore(u => u.PhoneNumberConfirmed);
            b.Ignore(u => u.TwoFactorEnabled);
        });

        builder.Entity<IdentityUserClaim<string>>(b => b.ToTable("UserClaims"));

        builder.Entity<IdentityUserLogin<string>>(b => b.ToTable("UserLogins", t => t.ExcludeFromMigrations()));

        builder.Entity<IdentityUserToken<string>>(b => b.ToTable("UserTokens", t => t.ExcludeFromMigrations()));

        builder.Entity<IdentityRole>(b =>
        {
            b.ToTable("Roles");
            b.HasData(UserData.SeedRoles());
        });

        builder.Entity<IdentityRoleClaim<string>>(b => b.ToTable("RoleClaims"));

        builder.Entity<UserRole>(b => b.ToTable("UserRoles"));

        builder.ApplyConfiguration(new UserAuditTrailConfiguration());
    }
}