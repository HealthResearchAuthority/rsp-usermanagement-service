using AutoFixture;
using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Identity;

namespace Rsp.UsersService.UnitTests.TestData;

[AttributeUsage(AttributeTargets.Method)]
public class FailedUpdateDataAttribute : AutoDataAttribute
{
    public FailedUpdateDataAttribute() : base(CreateFixture)
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

        fixture.Customize<IdentityError>
        (
            c => c
                .With(e => e.Code, fixture.Create<string>())
                .With(e => e.Description, fixture.Create<string>())
        );

        return fixture;
    }
}