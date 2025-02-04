using System.Security.Claims;
using AutoFixture;
using AutoFixture.Xunit2;

namespace Rsp.UsersService.UnitTests.TestData;

[AttributeUsage(AttributeTargets.Method)]
public class ClaimDataAttribute : AutoDataAttribute
{
    public ClaimDataAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<Claim>
        (
            c => c.FromFactory
            (
                () => new Claim(fixture.Create<string>(), fixture.Create<string>())
            )
        );

        return fixture;
    }
}
