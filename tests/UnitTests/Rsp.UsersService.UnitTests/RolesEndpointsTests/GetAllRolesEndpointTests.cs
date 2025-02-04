using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Rsp.UsersService.UnitTests.AsyncQueryHelper;
using Rsp.UsersService.UnitTests.TestData;
using Rsp.UsersService.WebApi.DTOs;
using Rsp.UsersService.WebApi.Endpoints.Roles;
using Rsp.UsersService.WebApi.Responses;
using Shouldly;

namespace Rsp.UsersService.UnitTests.RolesEndpointsTests;

public class GetAllRolesEndpointTests : TestServiceBase
{
    [Theory]
    [InlineData(0, 5)]
    [InlineData(5, 0)]
    [InlineData(-1, 10)]
    public async Task GetAllRoles_InvalidPageParameters_ReturnsBadRequest(int pageIndex, int pageSize)
    {
        var serviceProvider = Mocker.GetMock<IServiceProvider>();

        var result = await GetAllRolesEndpoint.GetAllRoles<IdentityRole>(serviceProvider.Object, pageIndex, pageSize);

        result.Result.ShouldBeOfType<BadRequest<string>>();
    }

    [Theory]
    [ValidRoleData]
    public async Task GetAllRoles_ValidParameters_ReturnsPaginatedRoles(int pageIndex, int pageSize, List<IdentityRole> roles)
    {
        pageIndex = Math.Abs(pageIndex) + 1;
        pageSize = Math.Abs(pageSize) + 1;

        var roleManager = Mocker.GetMock<RoleManager<IdentityRole>>();
        var asyncRoles = new MockAsyncEnumerable<IdentityRole>(roles);
        roleManager
            .Setup(x => x.Roles)
            .Returns(asyncRoles);

        Mocker
            .GetMock<IServiceProvider>()
            .Setup(x => x.GetService(typeof(RoleManager<IdentityRole>)))
            .Returns(roleManager.Object);

        var result = await GetAllRolesEndpoint.GetAllRoles<IdentityRole>
        (
            Mocker.Get<IServiceProvider>(),
            pageIndex,
            pageSize
        );

        var okResult = result.Result.ShouldBeOfType<Ok<AllRolesResponse>>();
        okResult
            .Value.ShouldNotBeNull()
            .TotalCount.ShouldBe(roles.Count);

        var expectedRoles = roles
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RoleDto(r.Id, r.Name!));

        okResult.Value.Roles.ShouldBe(expectedRoles, ignoreOrder: true);
    }
}