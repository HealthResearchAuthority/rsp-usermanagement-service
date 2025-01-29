using System.Security.Claims;
using AutoFixture;
using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Identity;
using Rsp.UsersService.WebApi.Requests;

namespace Rsp.UsersService.UnitTests.TestData;

[AttributeUsage(AttributeTargets.Method)]
public class ValidRemovalDataAttribute : AutoDataAttribute
{
    public ValidRemovalDataAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture();
        var claimType = fixture.Create<string>();
        var claimValue = fixture.Create<string>();

        fixture.Customize<IdentityRole>
        (
            c => c
                .With(r => r.Name, fixture.Create<string>())
        );

        fixture.Customize<RoleClaimRequest>
        (
            c => c
                .With(r => r.ClaimType, claimType)
                .With(r => r.ClaimValue, claimValue)
        );

        fixture.Customize<Claim>
        (
            c => c.FromFactory(() => new Claim(claimType, claimValue))
        );

        return fixture;
    }
}
