using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Moq;
using Rsp.UsersService.WebApi.Endpoints.Roles;
using Shouldly;

namespace Rsp.UsersService.UnitTests.RolesEndpointsTests;

public class CreateRoleEndpointTests : TestServiceBase
{
    [Fact]
    public async Task CreateRole_WithValidRoleName_ShouldCreateRoleSuccessfully()
    {
        // Arrange
        var roleName = "TestRole";
        var roleManager = Mocker.GetMock<RoleManager<IdentityRole>>();
        roleManager
            .Setup(rm => rm.CreateAsync(It.IsAny<IdentityRole>()))
            .ReturnsAsync(IdentityResult.Success);

        Mocker.Use(roleManager.Object);

        var serviceProvider = Mocker.GetMock<IServiceProvider>();
        serviceProvider
            .Setup(sp => sp.GetService(typeof(RoleManager<IdentityRole>)))
            .Returns(roleManager.Object);

        // Act
        var result = await CreateRoleEndpoint.CreateRole<IdentityRole>(serviceProvider.Object, roleName);

        // Assert
        result.Result.ShouldBeOfType<NoContent>();
        roleManager.Verify(rm => rm.CreateAsync(It.Is<IdentityRole>(r => r.Name == roleName)), Times.Once);
    }

    [Fact]
    public async Task CreateRole_WhenRoleCreationFails_ShouldReturnValidationProblem()
    {
        // Arrange
        var roleName = "TestRole";
        var roleManager = Mocker.GetMock<RoleManager<IdentityRole>>();
        roleManager
            .Setup(rm => rm.CreateAsync(It.IsAny<IdentityRole>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "ErrorCode", Description = "Role creation failed" }));

        Mocker.Use(roleManager.Object);

        var serviceProvider = Mocker.GetMock<IServiceProvider>();
        serviceProvider
            .Setup(sp => sp.GetService(typeof(RoleManager<IdentityRole>)))
            .Returns(roleManager.Object);

        // Act
        var result = await CreateRoleEndpoint.CreateRole<IdentityRole>(serviceProvider.Object, roleName);

        // Assert
        result.Result.ShouldBeOfType<ValidationProblem>();
        roleManager.Verify(rm => rm.CreateAsync(It.Is<IdentityRole>(r => r.Name == roleName)), Times.Once);
    }
}