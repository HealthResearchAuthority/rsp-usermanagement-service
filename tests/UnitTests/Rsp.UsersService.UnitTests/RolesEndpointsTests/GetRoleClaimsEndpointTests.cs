using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Moq;
using Rsp.UsersService.UnitTests.AsyncQueryHelper;
using Rsp.UsersService.UnitTests.TestData;
using Rsp.UsersService.WebApi.DTOs;
using Rsp.UsersService.WebApi.Endpoints.Roles;
using Shouldly;

namespace Rsp.UsersService.UnitTests.RolesEndpointsTests;

public class GetRoleClaimsEndpointTests : TestServiceBase
{
    [Theory]
    [InlineData("NonExistentRole")]
    public async Task GetRoleClaims_RoleNotFound_ReturnsNotFound(string roleName)
    {
        // Arrange
        var roleManager = Mocker.GetMock<RoleManager<IdentityRole>>();
        roleManager
            .Setup(x => x.Roles)
            .Returns(new MockAsyncEnumerable<IdentityRole>([]));

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(RoleManager<IdentityRole>)))
            .Returns(roleManager.Object);

        // Act
        var result = await GetRoleClaimsEndpoint.GetRoleClaims<IdentityRole>
        (
            Mocker.Get<IServiceProvider>(),
            roleName
        );

        // Assert
        result.Result.ShouldBeOfType<NotFound<string>>();
    }

    [Theory]
    [ClaimData]
    public async Task GetRoleClaims_RoleExists_ReturnsClaims(string roleName, List<Claim> testClaims)
    {
        // Arrange
        var role = new IdentityRole(roleName);
        var roles = new List<IdentityRole> { role };

        var roleManager = Mocker.GetMock<RoleManager<IdentityRole>>();
        roleManager
            .Setup(x => x.Roles)
            .Returns(new MockAsyncEnumerable<IdentityRole>(roles));

        roleManager
            .Setup(x => x.GetClaimsAsync(role))
            .ReturnsAsync(testClaims);

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(RoleManager<IdentityRole>)))
            .Returns(roleManager.Object);

        // Act
        var result = await GetRoleClaimsEndpoint.GetRoleClaims<IdentityRole>(
            Mocker.Get<IServiceProvider>(),
            roleName
        );

        // Assert
        var okResult = result.Result.ShouldBeOfType<Ok<IEnumerable<ClaimDto>>>();
        okResult.Value.ShouldBe
        (
            testClaims.Select(c => new ClaimDto(c.Type, c.Value)),
            ignoreOrder: true
        );
    }
}