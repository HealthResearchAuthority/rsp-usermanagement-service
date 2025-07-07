using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.UnitTests.AsyncQueryHelper;
using Rsp.UsersService.UnitTests.TestData;
using Rsp.UsersService.WebApi.DTOs;
using Rsp.UsersService.WebApi.Endpoints.Users;
using Rsp.UsersService.WebApi.Responses;
using Shouldly;

namespace Rsp.UsersService.UnitTests.UsersEndpointsTests;

public class SearchUsersEndpointTests : TestServiceBase
{
    [Theory]
    [InlineData(null, 0, 5)]
    public async Task GetAllUsers_InvalidPageParameters_ReturnsBadRequest(string? searchQuery, int pageIndex, int pageSize)
    {
        var userManager = Mocker.GetMock<UserManager<IrasUser>>();

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        var result = await SearchUsersEndpoint.SearchUsers<IrasUser>
        (
            Mocker.Get<IServiceProvider>(),
            searchQuery,
            null,
            pageIndex,
            pageSize
        );

        result.Result.ShouldBeOfType<ValidationProblem>();
    }

    [Theory]
    [ValidUserData]
    public async Task GetAllUsers_ValidRequest_ReturnsPaginatedUsers(
        string searchQuery,
        int pageIndex,
        int pageSize,
        List<IrasUser> users)
    {
        pageIndex = Math.Abs(pageIndex) + 1;
        pageSize = Math.Abs(pageSize) + 1;

        foreach (var user in users)
        {
            user.GivenName = "James" + Guid.NewGuid().ToString();
        }

        var userManager = Mocker.GetMock<UserManager<IrasUser>>();

        userManager
            .Setup(x => x.Users)
            .Returns(new MockAsyncEnumerable<IrasUser>(users));

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        var result = await SearchUsersEndpoint.SearchUsers<IrasUser>
        (
            Mocker.Get<IServiceProvider>(),
            "James",
            null,
            pageIndex,
            pageSize
        );

        var okResult = result.Result.ShouldBeOfType<Ok<AllUsersResponse>>();
        okResult
            .Value.ShouldNotBeNull()
            .TotalCount.ShouldBe(users.Count);

        var expectedUsers = users
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserDto(u.Id, u.GivenName, u.FamilyName, u.Email!, u.Title, u.JobTitle, u.Organisation, u.Telephone, u.Country, u.Status, u.LastUpdated, u.LastUpdated));

        okResult.Value.Users.ShouldBe(expectedUsers, ignoreOrder: true);
    }
}