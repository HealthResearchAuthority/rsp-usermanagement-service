using System.Security.Claims;
using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Moq;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.WebApi.Endpoints.Users;
using Rsp.UsersService.WebApi.Requests;
using Shouldly;

namespace Rsp.UsersService.UnitTests.UsersEndpointsTests;

public class RemoveUserClaimsEndpointTests : TestServiceBase
{
    [Theory, AutoData]
    public async Task Should_Return_NotFound_When_User_Not_Found(UserClaimsRequest claimsRequest)
    {
        // Arrange
        var userManager = Mocker.GetMock<UserManager<IrasUser>>();
        userManager
            .Setup(um => um.FindByEmailAsync(claimsRequest.Email))
            .ReturnsAsync((IrasUser?)null);

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(sp => sp.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        // Act
        var result = await RemoveUserClaimsEndpoint.RemoveUserClaims<IrasUser>
        (
            Mocker.Get<IServiceProvider>(),
            claimsRequest
        );

        // Assert
        var notFoundResult = result.Result.ShouldBeOfType<NotFound<string>>();
        notFoundResult.Value.ShouldBe($"User with email {claimsRequest.Email} not found");
    }

    [Theory, AutoData]
    public async Task Should_Return_ValidationProblem_When_Claims_Removal_Fails
    (
        UserClaimsRequest claimsRequest,
        IrasUser user
    )
    {
        // Arrange
        var userManager = Mocker.GetMock<UserManager<IrasUser>>();
        userManager
            .Setup(um => um.FindByEmailAsync(claimsRequest.Email))
            .ReturnsAsync(user);

        userManager
            .Setup(um => um.RemoveClaimsAsync(user, It.IsAny<IEnumerable<Claim>>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "ErrorCode", Description = "Claim removal failed" }));

        Mocker
            .GetMock<IServiceProvider>().Setup(sp => sp.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        // Act
        var result = await RemoveUserClaimsEndpoint.RemoveUserClaims<IrasUser>
        (
            Mocker.Get<IServiceProvider>(),
            claimsRequest
        );

        // Assert
        result.Result.ShouldBeOfType<ValidationProblem>();
    }

    [Theory, AutoData]
    public async Task Should_Return_NoContent_When_Claims_Removed_Successfully(
        UserClaimsRequest claimsRequest,
        IrasUser user
    )
    {
        // Arrange
        var userManager = Mocker.GetMock<UserManager<IrasUser>>();
        userManager
            .Setup(um => um.FindByEmailAsync(claimsRequest.Email))
            .ReturnsAsync(user);

        userManager
            .Setup(um => um.RemoveClaimsAsync(user, It.IsAny<IEnumerable<Claim>>()))
            .ReturnsAsync(IdentityResult.Success);

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(sp => sp.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        // Act
        var result = await RemoveUserClaimsEndpoint.RemoveUserClaims<IrasUser>
        (
            Mocker.Get<IServiceProvider>(),
            claimsRequest
        );

        // Assert
        result.Result.ShouldBeOfType<NoContent>();
    }
}