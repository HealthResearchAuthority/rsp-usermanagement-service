using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.Infrastructure;
using Rsp.UsersService.Infrastructure.Helpers;
using Shouldly;

namespace Rsp.UsersService.UnitTests.HelpersTests;

public class UserRoleAuditTrailHandlerTests
{
    private readonly UserRoleAuditTrailHandler _handler;

    public UserRoleAuditTrailHandlerTests()
    {
        _handler = new UserRoleAuditTrailHandler();
    }

    [Fact]
    public void CanHandle_ShouldReturnTrue_WhenEntityIsUserRole()
    {
        // Arrange
        var userRole = new UserRole();

        // Act
        var result = _handler.CanHandle(userRole);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void CanHandle_ShouldReturnFalse_WhenEntityIsNotUserRole()
    {
        // Arrange
        var nonUserRole = new object();

        // Act
        var result = _handler.CanHandle(nonUserRole);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task GenerateAuditTrails_ShouldReturnEmptyList_WhenEntityIsNotUserRole()
    {
        // Arrange
        var entity = new IrasUser { Id = "user1", Email = "test@example.com" };
        var systemAdmin = new IrasUser();

        var entityEntry = CreateFakeEntityEntry(entity);

        // Act
        var result = await _handler.GenerateAuditTrails(entityEntry, systemAdmin);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GenerateAuditTrails_ShouldGenerateAuditTrailForAddedEntity()
    {
        // Arrange
        var userRole = new UserRole { UserId = "user1", RoleId = "role1" };
        var systemAdmin = new IrasUser { Id = "admin1" };

        var user = new IrasUser
        {
            Id = "user1",
            Email = "test@example.com",
            FirstName = "first",
            LastName = "last",
            Status = "active"
        };

        var role = new IdentityRole
        {
            Id = "role1",
            NormalizedName = "ADMIN",
            Name = "admin"
        };

        var contextMock = CreateFakeDbContext(user, role);
        var entityEntry = CreateFakeEntityEntry(userRole, EntityState.Added);

        // Act
        var result = await _handler.GenerateAuditTrails(entityEntry, systemAdmin, contextMock);

        // Assert
        var auditTrail = result.Single();

        auditTrail.Description.ShouldBe("test@example.com was assigned admin role");
        auditTrail.UserId.ShouldBe("user1");
        auditTrail.SystemAdministratorId.ShouldBe("admin1");
    }

    [Fact]
    public async Task GenerateAuditTrails_ShouldGenerateAuditTrailForDeletedEntity()
    {
        // Arrange
        var userRole = new UserRole { UserId = "user1", RoleId = "role1" };
        var systemAdmin = new IrasUser { Id = "admin1" };

        var user = new IrasUser
        {
            Id = "user1",
            Email = "test@example.com",
            FirstName = "first",
            LastName = "last",
            Status = "active"
        };

        var role = new IdentityRole
        {
            Id = "role1",
            NormalizedName = "ADMIN",
            Name = "admin"
        };

        var contextMock = CreateFakeDbContext(user, role);
        var entityEntry = CreateFakeEntityEntry(userRole, EntityState.Deleted);

        // Act
        var result = await _handler.GenerateAuditTrails(entityEntry, systemAdmin, contextMock);

        // Assert
        var auditTrail = result.Single();

        auditTrail.Description.ShouldBe("test@example.com was unassigned admin role");
        auditTrail.UserId.ShouldBe("user1");
        auditTrail.SystemAdministratorId.ShouldBe("admin1");
    }

    [Fact]
    public async Task GenerateAuditTrails_ShouldReturnEmptyList_WhenContextIsNull()
    {
        // Arrange
        var userRole = new UserRole { UserId = "user1", RoleId = "role1" };
        var systemAdmin = new IrasUser { Id = "admin1" };
        var entityEntry = CreateFakeEntityEntry(userRole);

        // Act
        var result = await _handler.GenerateAuditTrails(entityEntry, systemAdmin, null);

        // Assert
        result.ShouldBeEmpty();
    }

    private static EntityEntry CreateFakeEntityEntry(object entity, EntityState state = EntityState.Unchanged)
    {
        var options = new DbContextOptionsBuilder<IrasIdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new IrasIdentityDbContext(options);
        var entry = context.Entry(entity);
        entry.State = state;

        return entry;
    }

    private static IrasIdentityDbContext CreateFakeDbContext(IrasUser user, IdentityRole role)
    {
        var options = new DbContextOptionsBuilder<IrasIdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new IrasIdentityDbContext(options);
        context.Users.Add(user);
        context.Roles.Add(role);
        context.SaveChanges();

        return context;
    }
}