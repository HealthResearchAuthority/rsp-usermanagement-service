using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rsp.UsersService.Domain.Entities;

namespace Rsp.UsersService.Infrastructure;

public class IrasIdentityDbContext(DbContextOptions<IrasIdentityDbContext> options) : IdentityDbContext<IrasUser>(options)
{
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
            b.HasData([
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "reviewer",
                    NormalizedName = "REVIEWER",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "user",
                    NormalizedName = "USER",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "question_set_admin",
                    NormalizedName = "QUESTION_SET_ADMIN",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                }
            ]);
        });

        builder.Entity<IdentityRoleClaim<string>>(b => b.ToTable("RoleClaims"));

        builder.Entity<IdentityUserRole<string>>(b => b.ToTable("UserRoles"));
    }
}