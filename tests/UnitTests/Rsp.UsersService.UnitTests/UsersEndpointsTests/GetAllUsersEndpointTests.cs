using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.UnitTests.AsyncQueryHelper;
using Rsp.UsersService.UnitTests.TestData;
using Rsp.UsersService.WebApi.DTOs;
using Rsp.UsersService.WebApi.Endpoints.Users;
using Rsp.UsersService.WebApi.Requests;
using Rsp.UsersService.WebApi.Responses;
using Shouldly;

namespace Rsp.UsersService.UnitTests.UsersEndpointsTests;

public class GetAllUsersEndpointTests : TestServiceBase
{
    [Theory]
    [InlineData("testQuery", 0, 5)]
    [InlineData("testQuery", 5, 0)]
    [InlineData(null, -1, 10)]
    public async Task GetAllUsers_InvalidPageParameters_ReturnsBadRequest(string? query, int pageIndex, int pageSize)
    {
        var userManager = Mocker.GetMock<UserManager<IrasUser>>();

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

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
    [ValidUserData]
    public async Task GetAllUsers_ValidRequest_ReturnsPaginatedUsers(
        int pageIndex,
        int pageSize,
        List<IrasUser> users)
    {
        foreach (var user in users)
        {
            user.GivenName += " testQuery";
            user.LastLogin = DateTime.UtcNow.AddDays(-1); // ensure FromDate/ToDate won't filter anything out
            user.Status = "Active";
        }

        pageIndex = Math.Abs(pageIndex) + 1;
        pageSize = Math.Abs(pageSize) + 1;

        var userManager = Mocker.GetMock<UserManager<IrasUser>>();

        userManager
            .Setup(x => x.Users)
            .Returns(new MockAsyncEnumerable<IrasUser>(users));

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        var searchRequest = new SearchUserRequest
        {
            SearchQuery = "testQuery",
            Status = true, // filter for 'Active'
            FromDate = DateTime.UtcNow.AddDays(-2),
            ToDate = DateTime.UtcNow.AddDays(1),
            Country = new List<string> { users.First().Country } // at least 1 country matches
        };

        var result = await GetAllUsersEndpoint.GetAllUsers<IrasUser>
        (
            Mocker.Get<IServiceProvider>(),
            searchRequest,
            pageIndex,
            pageSize
        );

        var okResult = result.Result.ShouldBeOfType<Ok<AllUsersResponse>>();
        okResult.Value.ShouldNotBeNull();

        var expectedUsers = users
            .Where(u => u.Country.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim().ToLowerInvariant())
                .Intersect(searchRequest.Country.Select(c => c.ToLowerInvariant()))
                .Any())
            .Where(u => u.Status == "Active")
            .Where(u => u.LastLogin >= searchRequest.FromDate && u.LastLogin <= searchRequest.ToDate)
            .Where(u => u.GivenName.ToLower().Contains("testquery")) // assuming GivenName is the match
            .OrderBy(u => u.GivenName)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserDto(
                u.Id, u.GivenName, u.FamilyName, u.Email!, u.Title, u.JobTitle,
                u.Organisation, u.Telephone, u.Country, u.Status, u.LastLogin,
                u.CurrentLogin, u.LastUpdated));

        okResult.Value.Users.ShouldBe(expectedUsers, true);
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
        var users = new List<IrasUser>
    {
        new() { Id = Guid.NewGuid().ToString(), GivenName = "Zane", FamilyName = "Smith", Email = "z@example.com", Status = "Active", CurrentLogin = DateTime.UtcNow.AddMinutes(-10) },
        new() { Id = Guid.NewGuid().ToString(), GivenName = "Amy", FamilyName = "Brown", Email = "a@example.com", Status = "Disabled", CurrentLogin = DateTime.UtcNow.AddMinutes(-20) },
        new() { Id = Guid.NewGuid().ToString(), GivenName = "John", FamilyName = "Adams", Email = "j@example.com", Status = "Active", CurrentLogin = DateTime.UtcNow.AddMinutes(-30) }
    };

        foreach (var user in users)
        {
            user.LastLogin = DateTime.UtcNow.AddDays(-1);
            user.Country = "England";
        }

        var userManager = Mocker.GetMock<UserManager<IrasUser>>();
        userManager.Setup(x => x.Users).Returns(new MockAsyncEnumerable<IrasUser>(users));

        Mocker.GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        var searchRequest = new SearchUserRequest
        {
            FromDate = DateTime.UtcNow.AddDays(-2),
            ToDate = DateTime.UtcNow.AddDays(1),
            Country = new List<string> { "England" }
        };

        // Act
        var result = await GetAllUsersEndpoint.GetAllUsers<IrasUser>(
            Mocker.Get<IServiceProvider>(),
            searchRequest,
            pageIndex: 1,
            pageSize: 10,
            sortField: sortField,
            sortDirection: sortDirection
        );

        // Assert
        var okResult = result.Result.ShouldBeOfType<Ok<AllUsersResponse>>();
        var returnedUsers = okResult.Value.Users.ToList();
        returnedUsers.Count.ShouldBe(users.Count);

        // Ensure sorted order
        var ordered = sortField switch
        {
            "givenname" => sortDirection == "asc" ?
                users.OrderBy(u => u.GivenName).ThenByDescending(u => u.CurrentLogin).ThenBy(u => u.Status) :
                users.OrderByDescending(u => u.GivenName).ThenByDescending(u => u.CurrentLogin).ThenBy(u => u.Status),

            "familyname" => sortDirection == "asc" ?
                users.OrderBy(u => u.FamilyName).ThenByDescending(u => u.CurrentLogin).ThenBy(u => u.Status) :
                users.OrderByDescending(u => u.FamilyName).ThenByDescending(u => u.CurrentLogin).ThenBy(u => u.Status),

            "email" => sortDirection == "asc" ?
                users.OrderBy(u => u.Email).ThenByDescending(u => u.CurrentLogin).ThenBy(u => u.Status) :
                users.OrderByDescending(u => u.Email).ThenByDescending(u => u.CurrentLogin).ThenBy(u => u.Status),

            "status" => sortDirection == "asc" ?
                users.OrderBy(u => u.Status).ThenByDescending(u => u.CurrentLogin) :
                users.OrderByDescending(u => u.Status).ThenByDescending(u => u.CurrentLogin).ThenBy(u => u.Status),

            "currentlogin" => sortDirection == "asc" ?
                users.OrderBy(u => u.CurrentLogin).ThenBy(u => u.Status) :
                users.OrderByDescending(u => u.CurrentLogin).ThenBy(u => u.Status),

            _ => users.OrderBy(u => u.GivenName).ThenByDescending(u => u.CurrentLogin).ThenBy(u => u.Status)
        };

        var expected = ordered
            .Select(u => new UserDto(u.Id, u.GivenName, u.FamilyName, u.Email!, u.Title, u.JobTitle,
                u.Organisation, u.Telephone, u.Country, u.Status, u.LastLogin,
                u.CurrentLogin, u.LastUpdated))
            .ToList();

        returnedUsers.ShouldBe(expected, ignoreOrder: false);
    }

}