using System.Security.Claims;
using AutoFixture;
using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Identity;
using Rsp.UsersService.WebApi.Requests;

namespace Rsp.UsersService.UnitTests.TestData;

[AttributeUsage(AttributeTargets.Method)]
public class ValidRoleClaimDataAttribute : AutoDataAttribute
{
    public ValidRoleClaimDataAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<IdentityRole>
        (
            c => c
                .With(r => r.Name, fixture.Create<string>())
        );

        fixture.Customize<RoleClaimRequest>
        (
            c => c
                .With(r => r.Role, fixture.Create<string>() + "Unique") // Ensure mismatch
                .With(r => r.ClaimType, fixture.Create<string>())
                .With(r => r.ClaimValue, fixture.Create<string>())
        );

        fixture.Customize<Claim>
        (
            c => c.FromFactory(() => new Claim(fixture.Create<string>(), fixture.Create<string>()))
        );

        return fixture;
    }
}
