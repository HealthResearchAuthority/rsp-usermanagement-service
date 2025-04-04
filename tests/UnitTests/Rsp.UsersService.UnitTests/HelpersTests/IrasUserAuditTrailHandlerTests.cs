using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.Infrastructure;
using Rsp.UsersService.Infrastructure.Helpers;

namespace Rsp.UsersService.UnitTests.HelpersTests;

public class IrasUserAuditTrailHandlerTests
{
    private readonly IrasUserAuditTrailHandler _handler;

    public IrasUserAuditTrailHandlerTests()
    {
        _handler = new IrasUserAuditTrailHandler();
    }

    [Fact]
    public void CanHandle_ShouldReturnTrue_WhenEntityIsIrasUser()
    {
        // Arrange
        var irasUser = new IrasUser();

        // Act
        var result = _handler.CanHandle(irasUser);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanHandle_ShouldReturnFalse_WhenEntityIsNotIrasUser()
    {
        // Arrange
        var nonIrasUser = new object();

        // Act
        var result = _handler.CanHandle(nonIrasUser);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GenerateAuditTrails_ShouldReturnEmptyList_WhenEntityIsNotIrasUser()
    {
        // Arrange
        var entity = new UserRole { UserId = "userId", RoleId = "roleId" };
        var systemAdmin = new IrasUser();

        var entityEntry = MockEntityEntry(entity);

        // Act
        var result = await _handler.GenerateAuditTrails(entityEntry, systemAdmin);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GenerateAuditTrails_ShouldGenerateAuditTrailForAddedEntity()
    {
        // Arrange
        var irasUser = new IrasUser { Id = "user1", Email = "test@example.com" };
        var systemAdmin = new IrasUser { Id = "admin1" };

        var entityEntry = MockEntityEntry(irasUser);

        // Act
        var result = await _handler.GenerateAuditTrails(entityEntry, systemAdmin);

        // Assert
        var auditTrail = Assert.Single(result);
        Assert.Equal("test@example.com was created", auditTrail.Description);
        Assert.Equal("user1", auditTrail.UserId);
        Assert.Equal("admin1", auditTrail.SystemAdministratorId);
    }

    [Fact]
    public async Task GenerateAuditTrails_ShouldGenerateAuditTrailForModifiedStatus()
    {
        // Arrange
        var irasUser = new IrasUser { Id = "user1", Email = "test@example.com", Status = "inactive" };
        var modifiedIrasUser = new IrasUser { Id = "user1", Email = "test@example.com", Status = "active" };
        var systemAdmin = new IrasUser { Id = "admin1" };

        var entityEntry = MockEntityEntry(irasUser, modifiedIrasUser);

        // Act
        var result = await _handler.GenerateAuditTrails(entityEntry, systemAdmin);

        // Assert
        var auditTrail = Assert.Single(result);
        Assert.Equal("test@example.com was enabled", auditTrail.Description);
        Assert.Equal("user1", auditTrail.UserId);
        Assert.Equal("admin1", auditTrail.SystemAdministratorId);
    }

    [Fact]
    public async Task GenerateAuditTrails_ShouldGenerateAuditTrailForModifiedProperty()
    {
        // Arrange
        var irasUser = new IrasUser { Id = "user1", Email = "test@example.com", FirstName = "old first" };
        var modifiedIrasUser = new IrasUser { Id = "user1", Email = "test@example.com", FirstName = "new first" };
        var systemAdmin = new IrasUser { Id = "admin1" };

        var entityEntry = MockEntityEntry(irasUser, modifiedIrasUser);

        // Act
        var result = await _handler.GenerateAuditTrails(entityEntry, systemAdmin);

        // Assert
        var auditTrail = Assert.Single(result);
        Assert.Equal("old first was changed to new first", auditTrail.Description);
        Assert.Equal("user1", auditTrail.UserId);
        Assert.Equal("admin1", auditTrail.SystemAdministratorId);
    }

    [Fact]
    public async Task GenerateAuditTrails_ShouldSkipUnchangedProperties()
    {
        // Arrange
        var irasUser = new IrasUser { Id = "user1", Email = "test@example.com" };
        var modifiedIrasUser = new IrasUser { Id = "user1", Email = "test@example.com" };
        var systemAdmin = new IrasUser { Id = "admin1" };

        var entityEntry = MockEntityEntry(irasUser, modifiedIrasUser);

        // Act
        var result = await _handler.GenerateAuditTrails(entityEntry, systemAdmin);

        // Assert
        Assert.Empty(result);
    }

    private static EntityEntry MockEntityEntry(object entity, object? modifiedEntity = null)
    {
        var options = new DbContextOptionsBuilder<IrasIdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new IrasIdentityDbContext(options);

        var entry = context.Entry(entity);

        if (modifiedEntity == null)
        {
            entry.State = EntityState.Added;
        }
        else
        {
            entry.State = EntityState.Modified;
            entry.CurrentValues.SetValues(modifiedEntity);
        }

        return entry;
    }
}