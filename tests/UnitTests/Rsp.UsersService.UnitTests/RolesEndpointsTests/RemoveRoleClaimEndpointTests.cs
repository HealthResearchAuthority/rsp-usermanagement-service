using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Moq;
using Rsp.UsersService.UnitTests.AsyncQueryHelper;
using Rsp.UsersService.UnitTests.TestData;
using Rsp.UsersService.WebApi.Endpoints.Roles;
using Rsp.UsersService.WebApi.Requests;
using Shouldly;

namespace Rsp.UsersService.UnitTests.RolesEndpointsTests;

public class RemoveRoleClaimEndpointTests : TestServiceBase
{
    [Theory]
    [InlineData("NonExistentRole", "claimType", "claimValue")]
    public async Task RemoveRoleClaim_RoleNotFound_ReturnsRoleNotFound(string roleName, string claimType, string claimValue)
    {
        // Arrange
        var request = new RoleClaimRequest
        {
            Role = roleName,
            ClaimType = claimType,
            ClaimValue = claimValue
        };

        var roleManager = Mocker.GetMock<RoleManager<IdentityRole>>();
        roleManager
            .Setup(x => x.Roles)
            .Returns(new MockAsyncEnumerable<IdentityRole>([]));

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(RoleManager<IdentityRole>)))
            .Returns(roleManager.Object);

        // Act
        var result = await RemoveRoleClaimEndpoint.RemoveRoleClaim<IdentityRole>(
            Mocker.Get<IServiceProvider>(),
            request
        );

        // Assert
        var notFoundResult = result.Result.ShouldBeOfType<NotFound<string>>();
        notFoundResult.Value.ShouldBe($"{roleName} not found");
    }

    [Theory]
    [ValidRoleClaimData]
    public async Task RemoveRoleClaim_ClaimNotFound_ReturnsClaimNotFound(RoleClaimRequest request, List<Claim> existingClaims)
    {
        // Arrange
        var role = new IdentityRole(request.Role);

        var roleManager = Mocker.GetMock<RoleManager<IdentityRole>>();

        roleManager
            .Setup(x => x.Roles)
            .Returns(new MockAsyncEnumerable<IdentityRole>([role]));

        roleManager
            .Setup(x => x.GetClaimsAsync(role))
            .ReturnsAsync(existingClaims);

        Mocker.GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(RoleManager<IdentityRole>)))
            .Returns(roleManager.Object);

        // Act
        var result = await RemoveRoleClaimEndpoint.RemoveRoleClaim<IdentityRole>
        (
            Mocker.Get<IServiceProvider>(),
            request
        );

        // Assert
        var notFoundResult = result.Result.ShouldBeOfType<NotFound<string>>();
        notFoundResult.Value.ShouldBe($"Claim with type {request.ClaimType} and value {request.ClaimValue} not found");
    }

    [Theory]
    [ValidRemovalData]
    public async Task RemoveRoleClaim_ValidRequest_ReturnsNoContent(RoleClaimRequest request, Claim targetClaim)
    {
        // Arrange
        var role = new IdentityRole(request.Role);

        var roleManager = Mocker.GetMock<RoleManager<IdentityRole>>();

        roleManager
            .Setup(x => x.Roles)
            .Returns(new MockAsyncEnumerable<IdentityRole>([role]));

        roleManager
            .Setup(x => x.GetClaimsAsync(role))
            .ReturnsAsync([targetClaim]);

        roleManager
            .Setup(x => x.RemoveClaimAsync(role, targetClaim))
            .ReturnsAsync(IdentityResult.Success);

        Mocker.GetMock<IServiceProvider>()

            .Setup(x => x.GetService(typeof(RoleManager<IdentityRole>)))
            .Returns(roleManager.Object);

        // Act
        var result = await RemoveRoleClaimEndpoint.RemoveRoleClaim<IdentityRole>
        (
            Mocker.Get<IServiceProvider>(),
            request
        );

        // Assert
        result.Result.ShouldBeOfType<NoContent>();
    }

    [Theory]
    [FailedRemovalData]
    public async Task RemoveRoleClaim_RemoveFails_ReturnsValidationProblem(RoleClaimRequest request, Claim targetClaim, IdentityError error)
    {
        // Arrange
        var role = new IdentityRole(request.Role);

        var roleManager = Mocker.GetMock<RoleManager<IdentityRole>>();

        roleManager
            .Setup(x => x.Roles)
            .Returns(new MockAsyncEnumerable<IdentityRole>([role]));

        roleManager
            .Setup(x => x.GetClaimsAsync(role))
            .ReturnsAsync([targetClaim]);

        roleManager
            .Setup(x => x.RemoveClaimAsync(role, targetClaim))
            .ReturnsAsync(IdentityResult.Failed(error));

        Mocker.GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(RoleManager<IdentityRole>)))
            .Returns(roleManager.Object);

        // Act
        var result = await RemoveRoleClaimEndpoint.RemoveRoleClaim<IdentityRole>(
            Mocker.Get<IServiceProvider>(),
            request
        );

        // Assert
        result.Result.ShouldBeOfType<ValidationProblem>();
    }
}