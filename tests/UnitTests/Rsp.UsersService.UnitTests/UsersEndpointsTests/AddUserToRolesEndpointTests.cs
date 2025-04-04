using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Moq;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.UnitTests.TestData;
using Rsp.UsersService.WebApi.Endpoints.Users;
using Shouldly;

namespace Rsp.UsersService.UnitTests.UsersEndpointsTests;

public class AddUserToRolesEndpointTests : TestServiceBase
{
    [Theory]
    [InlineData("nonexistent@example.com", "role1,role2")]
    public async Task AddUserToRoles_UserNotFound_ReturnsNotFound(string email, string roles)
    {
        // Arrange
        var userManager = Mocker.GetMock<UserManager<IrasUser>>();

        userManager
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync((IrasUser)null!);

        Mocker.GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        // Act
        var result = await AddUserToRolesEndpoint.AddUserToRoles<IrasUser>(
            Mocker.Get<IServiceProvider>(),
            email,
            roles
        );

        // Assert
        var notFoundResult = result.Result.ShouldBeOfType<NotFound<string>>();
        notFoundResult.Value.ShouldBe($"User with email {email} not found");
    }

    [Theory]
    [ValidRolesData]
    public async Task AddUserToRoles_ValidRequest_ReturnsNoContent
    (
        IrasUser user,
        string roles,
        List<string> existingRoles
    )
    {
        // Arrange
        var userManager = Mocker.GetMock<UserManager<IrasUser>>();

        userManager
            .Setup(x => x.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        userManager
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(existingRoles);

        userManager
            .Setup(x => x.AddToRolesAsync(
                user,
                It.Is<IEnumerable<string>>(r =>
                    r.All(role => roles.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(role) &&
                        !existingRoles.Contains(role))
                )
            ))
            .ReturnsAsync(IdentityResult.Success);

        Mocker.GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        // Act
        var result = await AddUserToRolesEndpoint.AddUserToRoles<IrasUser>(
            Mocker.Get<IServiceProvider>(),
            user.Email!,
            roles
        );

        // Assert
        result.Result.ShouldBeOfType<NoContent>();
    }

    [Theory]
    [FailedRolesData]
    public async Task AddUserToRoles_AddFails_ReturnsValidationProblem
    (
        IrasUser user,
        string roles,
        List<string> existingRoles,
        IdentityError error
    )
    {
        // Arrange
        var userManager = Mocker.GetMock<UserManager<IrasUser>>();

        userManager
            .Setup(x => x.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        userManager
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(existingRoles);

        userManager
            .Setup(x => x.AddToRolesAsync(
                user,
                It.Is<IEnumerable<string>>(r =>
                    r.All(role => roles.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(role) &&
                        !existingRoles.Contains(role))
                )
            ))
            .ReturnsAsync(IdentityResult.Failed(error));

        Mocker.GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        // Act
        var result = await AddUserToRolesEndpoint.AddUserToRoles<IrasUser>
        (
            Mocker.Get<IServiceProvider>(),
            user.Email!,
            roles
        );

        // Assert
        result.Result.ShouldBeOfType<ValidationProblem>();
    }
}