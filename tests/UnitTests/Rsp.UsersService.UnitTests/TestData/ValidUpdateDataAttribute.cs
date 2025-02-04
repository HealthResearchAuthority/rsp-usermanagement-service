using AutoFixture;
using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Identity;

namespace Rsp.UsersService.UnitTests.TestData;

[AttributeUsage(AttributeTargets.Method)]
public class ValidUpdateDataAttribute : AutoDataAttribute
{
    public ValidUpdateDataAttribute() : base(CreateFixture)
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

        return fixture;
    }
}