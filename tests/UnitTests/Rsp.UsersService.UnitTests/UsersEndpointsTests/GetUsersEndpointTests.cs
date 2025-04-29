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

public class GetUsersEndpointTests : TestServiceBase
{
    [Theory]
    [ValidUserData]
    public async Task GetAllUsers_ValidRequest_ReturnsPaginatedUsers(
        int pageIndex,
        int pageSize,
        List<IrasUser> users)
    {
        pageIndex = Math.Abs(pageIndex) + 1;
        pageSize = Math.Abs(pageSize) + 1;

        var userIds = users.Select(x => x.Id).ToList();

        foreach (var user in users)
        {
            user.LastName = "Smith-" + Guid.NewGuid().ToString();
        }

        var userManager = Mocker.GetMock<UserManager<IrasUser>>();

        userManager
            .Setup(x => x.Users)
            .Returns(new MockAsyncEnumerable<IrasUser>(users));

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        var result = await GetUsersEndpoint.GetUsersById<IrasUser>
        (
            Mocker.Get<IServiceProvider>(),
            userIds,
            "Smith",
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