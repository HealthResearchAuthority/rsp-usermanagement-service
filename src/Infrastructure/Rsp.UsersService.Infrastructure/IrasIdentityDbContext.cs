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

        builder.Entity<IrasUser>(b => b.ToTable("Users"));

        builder.Entity<IdentityUserClaim<string>>(b => b.ToTable("UserClaims"));

        builder.Entity<IdentityUserLogin<string>>(b => b.ToTable("UserLogins"));

        builder.Entity<IdentityUserToken<string>>(b => b.ToTable("UserTokens"));

        builder.Entity<IdentityRole>(b => b.ToTable("Roles"));

        builder.Entity<IdentityRoleClaim<string>>(b => b.ToTable("RoleClaims"));

        builder.Entity<IdentityUserRole<string>>(b => b.ToTable("UserRoles"));
    }
}