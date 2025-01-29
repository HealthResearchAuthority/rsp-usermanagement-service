using AutoFixture;
using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Identity;
using Rsp.UsersService.Domain.Entities;

namespace Rsp.UsersService.UnitTests.TestData;

[AttributeUsage(AttributeTargets.Method)]
public class FailedRolesDataAttribute : AutoDataAttribute
{
    public FailedRolesDataAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<IrasUser>
        (
            c => c.With(u => u.Email, fixture.Create<string>())
        );

        fixture.Customize<List<string>>
        (
            c => c.FromFactory(() => fixture.CreateMany<string>().ToList())
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
