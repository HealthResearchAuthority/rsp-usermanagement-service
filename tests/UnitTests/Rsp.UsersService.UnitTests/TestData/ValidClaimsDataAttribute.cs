using System.Security.Claims;
using AutoFixture;
using AutoFixture.Xunit2;
using Rsp.UsersService.Domain.Entities;

namespace Rsp.UsersService.UnitTests.TestData;

[AttributeUsage(AttributeTargets.Method)]
public class ValidClaimsDataAttribute : AutoDataAttribute
{
    public ValidClaimsDataAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<IrasUser>
        (
            c => c.With(u => u.Id, fixture.Create<string>())
        );

        fixture.Customize<Claim>
        (
            c => c.FromFactory(() => new Claim(fixture.Create<string>(), fixture.Create<string>()))
        );

        return fixture;
    }
}