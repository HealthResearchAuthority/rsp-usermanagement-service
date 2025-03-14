using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Moq;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.UnitTests.AsyncQueryHelper;
using Rsp.UsersService.WebApi.DTOs;
using Rsp.UsersService.WebApi.Endpoints.Users;
using Rsp.UsersService.WebApi.Responses;
using Shouldly;

namespace Rsp.UsersService.UnitTests.UsersEndpointsTests;

public class GetUsersInRoleEndpointTests : TestServiceBase
{
    [Theory, AutoData]
    public async Task Should_Return_NotFound_When_No_Users_In_Role(string roleName)
    {
        // Arrange
        var userManager = Mocker.GetMock<UserManager<IrasUser>>();

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(sp => sp.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        // Act
        var result = await GetUsersInRoleEndpoint.GetUsersInRole<IrasUser>
        (
            Mocker.Get<IServiceProvider>(),
            roleName
        );

        // Assert
        var notFoundResult = result.Result.ShouldBeOfType<NotFound<string>>();
        notFoundResult.Value.ShouldBe($"No users were found in {roleName} role");
    }

    [Theory, AutoData]
    public async Task Should_Return_Ok_With_Users_When_Users_Exist_In_Role
    (
        string roleName,
        IList<IrasUser> users
    )
    {
        // Arrange
        var userManager = Mocker.GetMock<UserManager<IrasUser>>();

        var irasUsers = new MockAsyncEnumerable<IrasUser>(users);

        userManager
            .Setup(um => um.Users)
            .Returns(irasUsers);

        userManager
            .Setup(um => um.GetUsersInRoleAsync(roleName))
            .ReturnsAsync(users);

        Mocker
            .GetMock<IServiceProvider>().Setup(sp => sp.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        // Act
        var result = await GetUsersInRoleEndpoint.GetUsersInRole<IrasUser>
        (
            Mocker.Get<IServiceProvider>(),
            roleName
        );

        // Assert
        var response = result.Result.ShouldBeOfType<Ok<AllUsersResponse>>();
        response
            .Value.ShouldNotBeNull()
            .Users.ShouldBeEquivalentTo(users.Select(user => new UserDto(user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email!,
                    user.Title,
                    user.JobTitle,
                    user.Organisation,
                    user.Telephone,
                    user.Country,
                    user.Status,
                    user.LastLogin,
                    user.LastUpdated)));

        response.Value.TotalCount.ShouldBe(users.Count);
    }
}