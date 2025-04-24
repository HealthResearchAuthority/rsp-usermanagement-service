using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.Infrastructure;
using Rsp.UsersService.Infrastructure.Helpers;

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
        Assert.True(result);
    }

    [Fact]
    public void CanHandle_ShouldReturnFalse_WhenEntityIsNotUserRole()
    {
        // Arrange
        var nonUserRole = new object();

        // Act
        var result = _handler.CanHandle(nonUserRole);

        // Assert
        Assert.False(result);
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
        Assert.Empty(result);
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
        var auditTrail = Assert.Single(result);
        Assert.Equal("test@example.com was assigned admin role", auditTrail.Description);
        Assert.Equal("user1", auditTrail.UserId);
        Assert.Equal("admin1", auditTrail.SystemAdministratorId);
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
        var auditTrail = Assert.Single(result);
        Assert.Equal("test@example.com was unassigned admin role", auditTrail.Description);
        Assert.Equal("user1", auditTrail.UserId);
        Assert.Equal("admin1", auditTrail.SystemAdministratorId);
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
        Assert.Empty(result);
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