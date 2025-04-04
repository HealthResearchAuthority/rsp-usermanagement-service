using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Moq;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.WebApi.Endpoints.Users;
using Rsp.UsersService.WebApi.Requests;
using Shouldly;

namespace Rsp.UsersService.UnitTests.UsersEndpointsTests;

public class RegisterUserEndpointTests : TestServiceBase
{
    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    public async Task RegisterUser_InvalidEmail_ReturnsValidationProblem(string email)
    {
        var registration = new UserRegisterRequest
        {
            Email = email,
            FirstName = "Test",
            LastName = "User"
        };

        var describer = new Mock<IdentityErrorDescriber>();
        describer
            .Setup(x => x.InvalidEmail(registration.Email))
            .Returns(new IdentityError { Code = "InvalidEmail", Description = "Invalid email" });

        var userManager = Mocker.GetMock<UserManager<IrasUser>>();

        userManager.Object.ErrorDescriber = describer.Object;

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        var result = await RegisterUserEndpoint.RegisterUser<IrasUser>
        (
            registration,
            Mocker.Get<IServiceProvider>()
        );

        result.Result.ShouldBeOfType<ValidationProblem>();
    }

    [Theory, AutoData]
    public async Task RegisterUser_ValidRequest_ReturnsNoContent(UserRegisterRequest registration)
    {
        var userStore = new Mock<IUserStore<IrasUser>>();
        var emailStore = userStore.As<IUserEmailStore<IrasUser>>();
        registration.Email = "test@example.com";

        var userManager = Mocker.GetMock<UserManager<IrasUser>>();

        userManager
            .Setup(x => x.CreateAsync(It.IsAny<IrasUser>()))
            .ReturnsAsync(IdentityResult.Success);

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        Mocker.GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(IUserStore<IrasUser>)))
            .Returns(emailStore.Object);

        var result = await RegisterUserEndpoint.RegisterUser<IrasUser>
        (
            registration,
            Mocker.Get<IServiceProvider>()
        );

        result.Result.ShouldBeOfType<NoContent>();

        userStore.Verify
        (
            x => x.SetUserNameAsync
            (
                It.IsAny<IrasUser>(),
                registration.Email,
                CancellationToken.None
            ), Times.Once
        );

        emailStore.Verify
        (
            x => x.SetEmailAsync
            (
                It.IsAny<IrasUser>(),
                registration.Email,
                CancellationToken.None
            ), Times.Once
        );
    }

    [Theory, AutoData]
    public async Task RegisterUser_CreateFails_ReturnsValidationProblem
    (
        UserRegisterRequest registration,
        IdentityError error
    )
    {
        var userStore = new Mock<IUserStore<IrasUser>>();
        var emailStore = userStore.As<IUserEmailStore<IrasUser>>();
        registration.Email = "test@example.com";

        var userManager = Mocker.GetMock<UserManager<IrasUser>>();
        userManager
            .Setup(x => x.CreateAsync(It.IsAny<IrasUser>()))
            .ReturnsAsync(IdentityResult.Failed(error));

        Mocker.GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(UserManager<IrasUser>)))
            .Returns(userManager.Object);

        Mocker.GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(IUserStore<IrasUser>)))
            .Returns(emailStore.Object);

        var result = await RegisterUserEndpoint.RegisterUser<IrasUser>
        (
            registration,
            Mocker.Get<IServiceProvider>()
        );

        result.Result.ShouldBeOfType<ValidationProblem>();
    }
}