using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Moq;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.UnitTests.TestData;
using Rsp.UsersService.WebApi.Endpoints.Users;
using Rsp.UsersService.WebApi.Requests;
using Shouldly;

namespace Rsp.UsersService.UnitTests.UsersEndpointsTests;

public class AddUserClaimsEndpointTests : TestServiceBase
{
    [Theory]
    [InlineData("nonexistent@example.com")]
    public async Task AddUserClaims_UserNotFound_ReturnsNotFound(string email)
    {
        // Arrange
        var request = new UserClaimsRequest
        {
            Email = email,
            Claims = []
        };

        var userManager = Mocker.GetMock<UserManager<IrasUser>>();
        userManager
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync((IrasUser)null!);

        Mocker.GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        // Act
        var result = await AddUserClaimsEndpoint.AddUserClaims<IrasUser>
        (
            Mocker.Get<IServiceProvider>(),
            request
        );

        // Assert
        var notFoundResult = result.Result.ShouldBeOfType<NotFound<string>>();
        notFoundResult.Value.ShouldBe($"User with email {email} not found");
    }

    [Theory]
    [ValidUserClaimsData]
    public async Task AddUserClaims_ValidRequest_ReturnsNoContent(IrasUser user, UserClaimsRequest request)
    {
        // Arrange
        var userManager = Mocker.GetMock<UserManager<IrasUser>>();

        userManager
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        userManager
            .Setup(x => x.AddClaimsAsync
            (
                user,
                It.Is<IEnumerable<Claim>>(claims =>
                    claims.All(c => request.Claims.Any(rc => rc.Key == c.Type && rc.Value == c.Value))
                )
            )).ReturnsAsync(IdentityResult.Success);

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        // Act
        var result = await AddUserClaimsEndpoint.AddUserClaims<IrasUser>
        (
            Mocker.Get<IServiceProvider>(),
            request
        );

        // Assert
        result.Result.ShouldBeOfType<NoContent>();
    }

    [Theory]
    [FailedUserClaimsData]
    public async Task AddUserClaims_AddFails_ReturnsValidationProblem(IrasUser user, UserClaimsRequest request, IdentityError error)
    {
        // Arrange
        var userManager = Mocker.GetMock<UserManager<IrasUser>>();

        userManager
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        userManager
            .Setup(x => x.AddClaimsAsync
            (
                user,
                It.Is<IEnumerable<Claim>>(claims =>
                    claims.All(c => request.Claims.Any(rc => rc.Key == c.Type && rc.Value == c.Value))
                )
            )).ReturnsAsync(IdentityResult.Failed(error));

        Mocker.GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        // Act
        var result = await AddUserClaimsEndpoint.AddUserClaims<IrasUser>
        (
            Mocker.Get<IServiceProvider>(),
            request
        );

        // Assert
        result.Result.ShouldBeOfType<ValidationProblem>();
    }
}