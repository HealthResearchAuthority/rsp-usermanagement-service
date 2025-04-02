using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Moq;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.WebApi.Endpoints.Users;
using Rsp.UsersService.WebApi.Requests;
using Shouldly;

namespace Rsp.UsersService.UnitTests.UsersEndpointsTests;

public class UpdateUserEndpointTests : TestServiceBase
{
    [Theory, AutoData]
    public async Task Should_Return_ValidationProblem_When_Email_Invalid
    (
        string email,
        UserRegisterRequest newUserDetails
    )
    {
        // Arrange
        newUserDetails.Email = "invalid-email";

        var describer = new Mock<IdentityErrorDescriber>();
        describer
            .Setup(x => x.InvalidEmail(newUserDetails.Email))
            .Returns(new IdentityError { Code = "InvalidEmail", Description = "Invalid email" });

        var userStore = new Mock<IUserStore<IrasUser>>();
        var emailStore = userStore.As<IUserEmailStore<IrasUser>>();

        var userManager = Mocker.GetMock<UserManager<IrasUser>>();

        userManager.Object.ErrorDescriber = describer.Object;

        Mocker.GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(IUserStore<IrasUser>)))
            .Returns(emailStore.Object);

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(sp => sp.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        var httpContext = new DefaultHttpContext();

        // Act
        var result = await UpdateUserEndpoint.UpdateUser<IrasUser>
        (
            email,
            newUserDetails,
            Mocker.Get<IServiceProvider>(),
            httpContext
        );

        // Assert
        result.Result.ShouldBeOfType<ValidationProblem>();
    }

    [Theory, AutoData]
    public async Task Should_Return_NotFound_When_User_Not_Found
    (
        string email,
        UserRegisterRequest newUserDetails
    )
    {
        // Arrange
        var userStore = new Mock<IUserStore<IrasUser>>();
        var emailStore = userStore.As<IUserEmailStore<IrasUser>>();
        newUserDetails.Email = "user@example.com";

        var userManager = Mocker.GetMock<UserManager<IrasUser>>();
        userManager
            .Setup(um => um.FindByEmailAsync(email))
            .ReturnsAsync((IrasUser?)null);

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(sp => sp.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        Mocker.GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(IUserStore<IrasUser>)))
            .Returns(emailStore.Object);

        var httpContext = new DefaultHttpContext();

        // Act
        var result = await UpdateUserEndpoint.UpdateUser<IrasUser>
        (
            email,
            newUserDetails,
            Mocker.Get<IServiceProvider>(),
            httpContext
        );

        // Assert
        var notFoundResult = result.Result.ShouldBeOfType<NotFound<string>>();
        notFoundResult.Value.ShouldBe($"{email} not found");
    }

    [Theory, AutoData]
    public async Task Should_Return_NoContent_When_User_Updated_Successfully
    (
        string email,
        UserRegisterRequest newUserDetails,
        IrasUser user
    )
    {
        // Arrange
        newUserDetails.Email = "user@example.com";
        var userManager = Mocker.GetMock<UserManager<IrasUser>>();
        userManager
            .Setup(um => um.FindByEmailAsync(email))
            .ReturnsAsync(user);

        userManager
            .Setup(um => um.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var userStore = new Mock<IUserStore<IrasUser>>();
        var emailStore = userStore.As<IUserEmailStore<IrasUser>>();

        emailStore
            .Setup(es => es.SetEmailAsync(user, newUserDetails.Email, CancellationToken.None))
            .Returns(Task.CompletedTask);

        userStore
            .Setup(us => us.SetUserNameAsync(user, newUserDetails.Email, CancellationToken.None))
            .Returns(Task.CompletedTask);

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(sp => sp.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(sp => sp.GetService(typeof(IUserStore<IrasUser>)))
            .Returns(userStore.Object);

        var httpContext = new DefaultHttpContext();

        // Act
        var result = await UpdateUserEndpoint.UpdateUser<IrasUser>
        (
            email,
            newUserDetails,
            Mocker.Get<IServiceProvider>(),
            httpContext
        );

        // Assert
        result.Result.ShouldBeOfType<NoContent>();
    }

    [Theory, AutoData]
    public async Task Should_Return_ValidationProblem_When_Update_Fails
    (
        string email,
        UserRegisterRequest newUserDetails,
        IrasUser user
    )
    {
        // Arrange
        newUserDetails.Email = "user@example.com";
        var userManager = Mocker.GetMock<UserManager<IrasUser>>();
        userManager
            .Setup(um => um.FindByEmailAsync(email))
            .ReturnsAsync(user);

        userManager
            .Setup(um => um.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "ErrorCode", Description = "Update failed" }));

        var userStore = new Mock<IUserStore<IrasUser>>();
        var emailStore = userStore.As<IUserEmailStore<IrasUser>>();

        emailStore
            .Setup(es => es.SetEmailAsync(user, newUserDetails.Email, CancellationToken.None))
            .Returns(Task.CompletedTask);

        userStore
            .Setup(us => us.SetUserNameAsync(user, newUserDetails.Email, CancellationToken.None))
            .Returns(Task.CompletedTask);

        Mocker
            .GetMock<IServiceProvider>().Setup(sp => sp.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        Mocker
            .GetMock<IServiceProvider>().Setup(sp => sp.GetService(typeof(IUserStore<IrasUser>)))
            .Returns(emailStore.Object);

        var httpContext = new DefaultHttpContext();

        // Act
        var result = await UpdateUserEndpoint.UpdateUser<IrasUser>
        (
            email,
            newUserDetails,
            Mocker.Get<IServiceProvider>(),
            httpContext
        );

        // Assert
        result.Result.ShouldBeOfType<ValidationProblem>();
    }
}