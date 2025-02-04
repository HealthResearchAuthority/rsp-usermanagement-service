using AutoFixture;
using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Identity;

namespace Rsp.UsersService.UnitTests.TestData;

[AttributeUsage(AttributeTargets.Method)]
public class ValidRoleDataAttribute : AutoDataAttribute
{
    public ValidRoleDataAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<IdentityRole>(c => c
            .With(r => r.Name, fixture.Create<string>())
            .With(r => r.Id, fixture.Create<string>()));

        fixture.Customize<int>(c => c.FromSeed(seed => Math.Abs(seed) + 1));

        return fixture;
    }
}
