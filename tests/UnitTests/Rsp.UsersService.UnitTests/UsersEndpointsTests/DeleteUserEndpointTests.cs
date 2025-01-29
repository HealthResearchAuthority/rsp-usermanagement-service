using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Moq;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.WebApi.Endpoints.Users;
using Shouldly;

namespace Rsp.UsersService.UnitTests.UsersEndpointsTests;

public class DeleteUserEndpointTests : TestServiceBase
{
    [Theory]
    [InlineData(null, null)]
    public async Task DeleteUser_MissingParameters_ReturnsValidationProblem(string? id, string? email)
    {
        var userManagerMock = Mocker.GetMock<UserManager<IrasUser>>();

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManagerMock.Object);

        var result = await DeleteUserEndpoint.DeleteUser<IrasUser>
        (
            Mocker.Get<IServiceProvider>(),
            id,
            email
        );

        result.Result.ShouldBeOfType<ValidationProblem>();
    }

    [Theory]
    [InlineData(null, "nonexistent@example.com")]
    [InlineData("nonexistent-id", null)]
    [InlineData("nonexistent-id", "nonexistent@example.com")]
    public async Task DeleteUser_UserNotFound_ReturnsNotFound(string? id, string? email)
    {
        var userManagerMock = Mocker.GetMock<UserManager<IrasUser>>();

        userManagerMock
            .Setup(x => x.FindByEmailAsync(email!))
            .ReturnsAsync((IrasUser)null!);

        userManagerMock
            .Setup(x => x.FindByIdAsync(id!))
            .ReturnsAsync((IrasUser)null!);

        Mocker.GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManagerMock.Object);

        var result = await DeleteUserEndpoint.DeleteUser<IrasUser>
        (
            Mocker.Get<IServiceProvider>(),
            id,
            email
        );

        var notFoundResult = result.Result.ShouldBeOfType<NotFound<string>>();
        var expectedMessage = (id, email) switch
        {
            (null, not null) => $"User with email {email} not found",
            (not null, null) => $"User with id {id} not found",
            (not null, not null) => $"User with id {id} or email {email} not found",
            _ => "User not found"
        };

        notFoundResult.Value.ShouldBe(expectedMessage);
    }

    [Theory]
    [AutoData]
    public async Task DeleteUser_ValidRequest_ReturnsNoContent(IrasUser user)
    {
        var userManagerMock = Mocker.GetMock<UserManager<IrasUser>>();

        userManagerMock
            .Setup(x => x.FindByIdAsync(user.Id))
            .ReturnsAsync(user);

        userManagerMock
            .Setup(x => x.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        Mocker.GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManagerMock.Object);

        var result = await DeleteUserEndpoint.DeleteUser<IrasUser>
        (
            Mocker.Get<IServiceProvider>(),
            user.Id,
            null
        );

        result.Result.ShouldBeOfType<NoContent>();
    }

    [Theory]
    [AutoData]
    public async Task DeleteUser_DeleteFails_ReturnsValidationProblem(IrasUser user, IdentityError error)
    {
        var userManagerMock = Mocker.GetMock<UserManager<IrasUser>>();

        userManagerMock
            .Setup(x => x.FindByIdAsync(user.Id))
            .ReturnsAsync(user);

        userManagerMock
            .Setup(x => x.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Failed(error));

        Mocker.GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManagerMock.Object);

        var result = await DeleteUserEndpoint.DeleteUser<IrasUser>
        (
            Mocker.Get<IServiceProvider>(),
            user.Id,
            null
        );

        result.Result.ShouldBeOfType<ValidationProblem>();
    }
}