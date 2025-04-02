using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Moq;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.WebApi.Endpoints.Users;
using Shouldly;

namespace Rsp.UsersService.UnitTests.UsersEndpointsTests;

public class RemoveUserFromRolesEndpointTests : TestServiceBase
{
    [Theory, AutoData]
    public async Task Should_Return_NotFound_When_User_Not_Found(string email, string roles)
    {
        // Arrange
        var userManager = Mocker.GetMock<UserManager<IrasUser>>();
        userManager
            .Setup(um => um.FindByEmailAsync(email))
            .ReturnsAsync((IrasUser?)null);

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(sp => sp.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        var httpContext = new DefaultHttpContext();

        // Act
        var result = await RemoveUserFromRolesEndpoint.RemoveUserFromRoles<IrasUser>
        (
            Mocker.Get<IServiceProvider>(),
            httpContext,
            email,
            roles
        );

        // Assert
        var notFoundResult = result.Result.ShouldBeOfType<NotFound<string>>();
        notFoundResult.Value.ShouldBe($"User with email {email} not found");
    }

    [Theory, AutoData]
    public async Task Should_Return_NoContent_When_User_Removed_From_Roles_Successfully
    (
        string email,
        string roles,
        IrasUser user
    )
    {
        // Arrange
        var userManager = Mocker.GetMock<UserManager<IrasUser>>();
        userManager
            .Setup(um => um.FindByEmailAsync(email))
            .ReturnsAsync(user);

        userManager
            .Setup(um => um.GetRolesAsync(user))
            .ReturnsAsync(roles.Split(','));

        userManager
            .Setup(um => um.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(sp => sp.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        var httpContext = new DefaultHttpContext();

        // Act
        var result = await RemoveUserFromRolesEndpoint.RemoveUserFromRoles<IrasUser>
        (
            Mocker.Get<IServiceProvider>(),
            httpContext,
            email,
            roles
        );

        // Assert
        result.Result.ShouldBeOfType<NoContent>();
    }

    [Theory, AutoData]
    public async Task Should_Return_NoContent_When_User_Has_No_Matching_Roles(
        string email,
        string roles,
        IrasUser user
    )
    {
        // Arrange
        var userManager = Mocker.GetMock<UserManager<IrasUser>>();
        userManager
            .Setup(um => um.FindByEmailAsync(email))
            .ReturnsAsync(user);

        userManager
            .Setup(um => um.GetRolesAsync(user))
            .ReturnsAsync([]);

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(sp => sp.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        var httpContext = new DefaultHttpContext();

        // Act
        var result = await RemoveUserFromRolesEndpoint.RemoveUserFromRoles<IrasUser>
        (
            Mocker.Get<IServiceProvider>(),
            httpContext,
            email,
            roles
        );

        // Assert
        result.Result.ShouldBeOfType<NoContent>();
    }

    [Theory, AutoData]
    public async Task Should_Return_ValidationProblem_When_Role_Removal_Fails
    (
        string email,
        string roles,
        IrasUser user
    )
    {
        // Arrange
        var userManager = Mocker.GetMock<UserManager<IrasUser>>();
        userManager
            .Setup(um => um.FindByEmailAsync(email))
            .ReturnsAsync(user);

        userManager
            .Setup(um => um.GetRolesAsync(user))
            .ReturnsAsync(roles.Split(','));

        userManager
            .Setup(um => um.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "ErroCode", Description = "Role removal failed" }));

        Mocker
            .GetMock<IServiceProvider>().Setup(sp => sp.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        var httpContext = new DefaultHttpContext();

        // Act
        var result = await RemoveUserFromRolesEndpoint.RemoveUserFromRoles<IrasUser>
        (
            Mocker.Get<IServiceProvider>(),
            httpContext,
            email,
            roles
        );

        // Assert
        result.Result.ShouldBeOfType<ValidationProblem>();
    }
}