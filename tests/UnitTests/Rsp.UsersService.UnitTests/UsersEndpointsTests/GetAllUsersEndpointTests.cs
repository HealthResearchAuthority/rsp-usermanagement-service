using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.Infrastructure;
using Rsp.UsersService.WebApi.Endpoints.Users;
using Rsp.UsersService.WebApi.Requests;
using Rsp.UsersService.WebApi.Responses;
using Shouldly;

namespace Rsp.UsersService.UnitTests.UsersEndpointsTests;

public class GetAllUsersEndpointTests : TestServiceBase
{
    // Helper to create a fresh in-memory context
    private static IrasIdentityDbContext CreateDb(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<IrasIdentityDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new IrasIdentityDbContext(options);
    }

    // Helper to wire the db into the mocked IServiceProvider
    private void RegisterDbOnServiceProvider(IrasIdentityDbContext db)
    {
        Mocker.GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(IrasIdentityDbContext)))
            .Returns(db);
    }

    [Theory]
    [InlineData("testQuery", 0, 5)]
    [InlineData("testQuery", 5, 0)]
    [InlineData(null, -1, 10)]
    public async Task GetAllUsers_InvalidPageParameters_ReturnsBadRequest(string? query, int pageIndex, int pageSize)
    {
        // No db registration required because endpoint returns before resolving services
        var searchRequest = query != null ? new SearchUserRequest { SearchQuery = query } : null;

        var result = await GetAllUsersEndpoint.GetAllUsers<IrasUser>
        (
            Mocker.Get<IServiceProvider>(),
            searchRequest,
            pageIndex,
            pageSize
        );

        var badRequestResult = result.Result.ShouldBeOfType<BadRequest<string>>();
        badRequestResult.Value.ShouldBe("PageIndex and PageSize should be greater than 0");
    }

    [Theory]
    [InlineData("givenname", "asc")]
    [InlineData("givenname", "desc")]
    [InlineData("familyname", "asc")]
    [InlineData("familyname", "desc")]
    [InlineData("email", "asc")]
    [InlineData("email", "desc")]
    [InlineData("status", "asc")]
    [InlineData("status", "desc")]
    [InlineData("currentlogin", "asc")]
    [InlineData("currentlogin", "desc")]
    public async Task GetAllUsers_SortsCorrectly(string sortField, string sortDirection)
    {
        // Arrange
        var seeded = new List<IrasUser>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(), GivenName = "Zane", FamilyName = "Smith", Email = "z@example.com",
                Status = "Active", CurrentLogin = DateTime.UtcNow.AddMinutes(-10), Country = "England"
            },
            new()
            {
                Id = Guid.NewGuid().ToString(), GivenName = "Amy", FamilyName = "Brown", Email = "a@example.com",
                Status = "Disabled", CurrentLogin = DateTime.UtcNow.AddMinutes(-20), Country = "England"
            },
            new()
            {
                Id = Guid.NewGuid().ToString(), GivenName = "John", FamilyName = "Adams", Email = "j@example.com",
                Status = "Active", CurrentLogin = DateTime.UtcNow.AddMinutes(-30), Country = "England"
            }
        };

        using var db = CreateDb();
        db.Set<IrasUser>().AddRange(seeded);
        await db.SaveChangesAsync();

        RegisterDbOnServiceProvider(db);

        var searchRequest = new SearchUserRequest
        {
            // Date filters use CurrentLogin
            FromDate = DateTime.UtcNow.AddDays(-2),
            ToDate = DateTime.UtcNow.AddDays(1),
            Country = new List<string> { "England" },
        };

        // Act
        var result = await GetAllUsersEndpoint.GetAllUsers<IrasUser>(
            Mocker.Get<IServiceProvider>(),
            searchRequest,
            1,
            10,
            sortField,
            sortDirection
        );

        // Assert
        var okResult = result.Result.ShouldBeOfType<Ok<AllUsersResponse>>();
        var returnedUsers = okResult.Value.Users.ToList();
        returnedUsers.Count.ShouldBe(seeded.Count);

        // Validate primary-field sorting without assuming secondary tie-breakers
        Func<IrasUser, object?> keySelector = sortField switch
        {
            "givenname" => u => u.GivenName,
            "familyname" => u => u.FamilyName,
            "email" => u => u.Email,
            "status" => u => u.Status,
            "currentlogin" => u => u.CurrentLogin,
            _ => u => u.GivenName
        };

        var expectedOrder = sortDirection?.ToLowerInvariant() switch
        {
            "desc" => seeded.OrderByDescending(keySelector).Select(u => u.Id).ToList(),
            _ => seeded.OrderBy(keySelector).Select(u => u.Id).ToList()
        };

        var actualOrder = returnedUsers.Select(u => u.Id).ToList();
        actualOrder.ShouldBe(expectedOrder, false);
    }
}