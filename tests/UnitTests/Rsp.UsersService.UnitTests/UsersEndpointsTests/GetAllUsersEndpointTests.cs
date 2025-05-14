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

public class GetAllUsersEndpointTests : TestServiceBase
{
    [Theory]
    [InlineData(0, 5)]
    [InlineData(5, 0)]
    [InlineData(-1, 10)]
    public async Task GetAllUsers_InvalidPageParameters_ReturnsBadRequest(int pageIndex, int pageSize)
    {
        var userManager = Mocker.GetMock<UserManager<IrasUser>>();

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        var result = await GetAllUsersEndpoint.GetAllUsers<IrasUser>
        (
            Mocker.Get<IServiceProvider>(),
            null,
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

        var result = await GetAllUsersEndpoint.GetAllUsers<IrasUser>
        (
            Mocker.Get<IServiceProvider>(),
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
            .Select(u => new UserDto(u.Id, u.FirstName, u.LastName, u.Email!, u.Title, u.JobTitle, u.Organisation, u.Telephone, u.Country, u.Status, u.LastUpdated, u.LastUpdated));

        okResult.Value.Users.ShouldBe(expectedUsers, ignoreOrder: true);
    }
}