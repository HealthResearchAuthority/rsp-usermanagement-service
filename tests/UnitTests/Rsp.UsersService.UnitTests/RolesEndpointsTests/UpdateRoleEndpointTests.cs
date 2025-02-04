using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Moq;
using Rsp.UsersService.UnitTests.TestData;
using Rsp.UsersService.WebApi.Endpoints.Roles;
using Shouldly;

namespace Rsp.UsersService.UnitTests.RolesEndpointsTests;

public class UpdateRoleEndpointTests : TestServiceBase
{
    [Theory]
    [InlineData("NonExistentRole", "NewRoleName")]
    public async Task UpdateRole_RoleNotFound_ReturnsNotFound(string roleName, string newName)
    {
        // Arrange
        var roleManager = Mocker.GetMock<RoleManager<IdentityRole>>();

        roleManager
            .Setup(x => x.FindByNameAsync(roleName))
            .ReturnsAsync((IdentityRole?)null);

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(RoleManager<IdentityRole>)))
            .Returns(roleManager.Object);

        // Act
        var result = await UpdateRoleEndpoint.UpdateRole<IdentityRole>(
            Mocker.Get<IServiceProvider>(),
            roleName,
            newName
        );

        // Assert
        var notFoundResult = result.Result.ShouldBeOfType<NotFound<string>>();
        notFoundResult.Value.ShouldBe($"{roleName} not found");
    }

    [Theory]
    [ValidUpdateData]
    public async Task UpdateRole_ValidRequest_ReturnsNoContent(IdentityRole role, string newName)
    {
        // Arrange
        var roleManager = Mocker.GetMock<RoleManager<IdentityRole>>();

        roleManager
            .Setup(x => x.FindByNameAsync(role.Name!))
            .ReturnsAsync(role);

        roleManager
            .Setup(x => x.UpdateAsync(It.Is<IdentityRole>(r => r.Name == newName)))
            .ReturnsAsync(IdentityResult.Success);

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(RoleManager<IdentityRole>)))
            .Returns(roleManager.Object);

        // Act
        var result = await UpdateRoleEndpoint.UpdateRole<IdentityRole>
        (
            Mocker.Get<IServiceProvider>(),
            role.Name!,
            newName
        );

        // Assert
        result.Result.ShouldBeOfType<NoContent>();
    }

    [Theory]
    [FailedUpdateData]
    public async Task UpdateRole_UpdateFails_ReturnsValidationProblem(IdentityRole role, string newName, IdentityError error)
    {
        // Arrange
        var roleManager = Mocker.GetMock<RoleManager<IdentityRole>>();

        roleManager
            .Setup(x => x.FindByNameAsync(role.Name!))
            .ReturnsAsync(role);

        roleManager
            .Setup(x => x.UpdateAsync(It.Is<IdentityRole>(r => r.Name == newName)))
            .ReturnsAsync(IdentityResult.Failed(error));

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(RoleManager<IdentityRole>)))
            .Returns(roleManager.Object);

        // Act
        var result = await UpdateRoleEndpoint.UpdateRole<IdentityRole>
        (
            Mocker.Get<IServiceProvider>(),
            role.Name!,
            newName
        );

        // Assert
        result.Result.ShouldBeOfType<ValidationProblem>();
    }
}