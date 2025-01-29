using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Moq;
using Rsp.UsersService.WebApi.Endpoints.Roles;
using Shouldly;

namespace Rsp.UsersService.UnitTests.RolesEndpointsTests;

public class DeleteRoleEndpointTests : TestServiceBase
{
    [Fact]
    public async Task DeleteRole_NonExistentRole_ShouldReturnNotFound()
    {
        // Arrange
        var roleName = "NonExistentRole";
        var roleManager = Mocker.GetMock<RoleManager<IdentityRole>>();
        roleManager
            .Setup(rm => rm.FindByNameAsync(roleName))
            .ReturnsAsync((IdentityRole?)null);

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider
            .Setup(sp => sp.GetService(typeof(RoleManager<IdentityRole>)))
            .Returns(roleManager.Object);

        // Act
        var result = await DeleteRoleEndpoint.DeleteRole<IdentityRole>(serviceProvider.Object, roleName);

        // Assert
        var notFoundResult = result.Result.ShouldBeOfType<NotFound<string>>();
        notFoundResult.Value.ShouldBe($"{roleName} not found");
    }

    [Fact]
    public async Task DeleteRole_ExistingRole_ShouldReturnNoContent()
    {
        // Arrange
        var roleName = "ExistingRole";
        var role = new IdentityRole(roleName);
        var roleManager = Mocker.GetMock<RoleManager<IdentityRole>>();
        roleManager
            .Setup(rm => rm.FindByNameAsync(roleName))
            .ReturnsAsync(role);
        roleManager
            .Setup(rm => rm.DeleteAsync(role))
            .ReturnsAsync(IdentityResult.Success);

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider
            .Setup(sp => sp.GetService(typeof(RoleManager<IdentityRole>)))
            .Returns(roleManager.Object);

        // Act
        var result = await DeleteRoleEndpoint.DeleteRole<IdentityRole>(serviceProvider.Object, roleName);

        // Assert
        result.Result.ShouldBeOfType<NoContent>();
        roleManager.Verify(rm => rm.DeleteAsync(role), Times.Once);
    }

    [Fact]
    public async Task DeleteRole_DeletionFails_ShouldReturnValidationProblem()
    {
        // Arrange
        var roleName = "ExistingRole";
        var role = new IdentityRole(roleName);
        var roleManager = Mocker.GetMock<RoleManager<IdentityRole>>();
        roleManager
            .Setup(rm => rm.FindByNameAsync(roleName))
            .ReturnsAsync(role);
        roleManager
            .Setup(rm => rm.DeleteAsync(role))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "ErrorCode", Description = "Deletion failed" }));

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider
            .Setup(sp => sp.GetService(typeof(RoleManager<IdentityRole>)))
            .Returns(roleManager.Object);

        // Act
        var result = await DeleteRoleEndpoint.DeleteRole<IdentityRole>(serviceProvider.Object, roleName);

        // Assert
        result.Result.ShouldBeOfType<ValidationProblem>();
        roleManager.Verify(rm => rm.DeleteAsync(It.Is<IdentityRole>(r => r.Name == roleName)), Times.Once);
    }
}