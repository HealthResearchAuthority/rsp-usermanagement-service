using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Moq;
using Rsp.UsersService.WebApi.Endpoints.Roles;
using Rsp.UsersService.WebApi.Requests;

namespace Rsp.UsersService.UnitTests.RolesEndpointsTests;

public class AddRoleClaimEndpointTests : TestServiceBase
{
    [Fact]
    public async Task ShouldReturnNotFoundWhenRoleDoesNotExist()
    {
        // Arrange
        var serviceProvider = Mocker.GetMock<IServiceProvider>();
        var roleManager = Mocker.GetMock<RoleManager<IdentityRole>>();

        var claimRequest = new RoleClaimRequest
        {
            Role = "NonExistentRole",
            ClaimType = "SomeClaimType",
            ClaimValue = "SomeClaimValue"
        };

        roleManager
            .Setup(rm => rm.Roles)
            .Returns(new List<IdentityRole>().AsQueryable());

        serviceProvider
            .Setup(sp => sp.GetService(typeof(RoleManager<IdentityRole>)))
            .Returns(roleManager.Object);

        // Act
        var result = await AddRoleClaimEndpoint.AddRoleClaim<IdentityRole>(serviceProvider.Object, claimRequest);

        // Assert
        Assert.IsType<NotFound<string>>(result.Result);
        Assert.Equal("NonExistentRole not found", ((NotFound<string>)result.Result).Value);
    }

    [Fact]
    public async Task ShouldReturnNoContentWhenClaimIsSuccessfullyAddedToExistingRole()
    {
        // Arrange
        var serviceProvider = Mocker.GetMock<IServiceProvider>();
        var roleManager = Mocker.GetMock<RoleManager<IdentityRole>>();

        var existingRole = new IdentityRole { Name = "ExistingRole" };
        var claimRequest = new RoleClaimRequest
        {
            Role = "ExistingRole",
            ClaimType = "SomeClaimType",
            ClaimValue = "SomeClaimValue"
        };

        roleManager
            .Setup(rm => rm.Roles)
            .Returns(new List<IdentityRole> { existingRole }.AsQueryable());

        roleManager
            .Setup(rm => rm.AddClaimAsync(existingRole, It.IsAny<Claim>()))
            .ReturnsAsync(IdentityResult.Success);

        serviceProvider
            .Setup(sp => sp.GetService(typeof(RoleManager<IdentityRole>)))
            .Returns(roleManager.Object);

        // Act
        var result = await AddRoleClaimEndpoint.AddRoleClaim<IdentityRole>(serviceProvider.Object, claimRequest);

        // Assert
        Assert.IsType<NoContent>(result.Result);
    }

    [Fact]
    public async Task ShouldReturnValidationProblemWhenClaimAdditionResultsInErrors()
    {
        // Arrange
        var serviceProvider = Mocker.GetMock<IServiceProvider>();
        var roleManager = Mocker.GetMock<RoleManager<IdentityRole>>();

        var existingRole = new IdentityRole { Name = "ExistingRole" };
        var claimRequest = new RoleClaimRequest
        {
            Role = "ExistingRole",
            ClaimType = "SomeClaimType",
            ClaimValue = "SomeClaimValue"
        };

        roleManager
            .Setup(rm => rm.Roles)
            .Returns(new List<IdentityRole> { existingRole }.AsQueryable());

        var identityError = new IdentityError { Code = "ClaimErrorCode", Description = "Error adding claim" };
        roleManager
            .Setup(rm => rm.AddClaimAsync(existingRole, It.IsAny<Claim>()))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        serviceProvider
            .Setup(sp => sp.GetService(typeof(RoleManager<IdentityRole>)))
            .Returns(roleManager.Object);

        // Act
        var result = await AddRoleClaimEndpoint.AddRoleClaim<IdentityRole>(serviceProvider.Object, claimRequest);

        // Assert
        Assert.IsType<ValidationProblem>(result.Result);
    }
}