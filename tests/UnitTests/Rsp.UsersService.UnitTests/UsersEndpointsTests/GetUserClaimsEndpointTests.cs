using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Moq;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.UnitTests.TestData;
using Rsp.UsersService.WebApi.DTOs;
using Rsp.UsersService.WebApi.Endpoints.Users;
using Shouldly;

namespace Rsp.UsersService.UnitTests.UsersEndpointsTests;

public class GetUserClaimsEndpointTests : TestServiceBase
{
    [Theory]
    [InlineData(null, null)]
    public async Task GetUserClaims_MissingParameters_ReturnsValidationProblem(string? id, string? email)
    {
        var userManagerMock = Mocker.GetMock<UserManager<IrasUser>>();

        Mocker.GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManagerMock.Object);

        var result = await GetUserClaimsEndpoint.GetUserClaims<IrasUser>
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
    public async Task GetUserClaims_UserNotFound_ReturnsNotFound(string? id, string? email)
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

        var result = await GetUserClaimsEndpoint.GetUserClaims<IrasUser>
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

    [Theory, ValidClaimsData]
    public async Task GetUserClaims_ValidRequest_ReturnsClaims
    (
        IrasUser user,
        List<Claim> claims
    )
    {
        var userManagerMock = Mocker.GetMock<UserManager<IrasUser>>();

        userManagerMock
            .Setup(x => x.FindByIdAsync(user.Id))
            .ReturnsAsync(user);

        userManagerMock
            .Setup(x => x.GetClaimsAsync(user))
            .ReturnsAsync(claims);

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManagerMock.Object);

        var result = await GetUserClaimsEndpoint.GetUserClaims<IrasUser>
        (
            Mocker.Get<IServiceProvider>(),
            user.Id,
            null
        );

        var okResult = result.Result.ShouldBeOfType<Ok<IEnumerable<ClaimDto>>>();
        okResult.Value.ShouldBe(
            claims.Select(c => new ClaimDto(c.Type, c.Value)),
            ignoreOrder: true
        );
    }
}