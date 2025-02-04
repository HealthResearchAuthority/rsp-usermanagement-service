using AutoFixture;
using AutoFixture.Xunit2;
using Rsp.UsersService.Domain.Entities;

namespace Rsp.UsersService.UnitTests.TestData;

[AttributeUsage(AttributeTargets.Method)]
public class ValidUserDataAttribute : AutoDataAttribute
{
    public ValidUserDataAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<IrasUser>
        (
            c => c
                .With(u => u.Id, fixture.Create<string>())
                .With(u => u.FirstName, fixture.Create<string>())
                .With(u => u.LastName, fixture.Create<string>())
                .With(u => u.Email, fixture.Create<string>())
        );

        fixture.Customize<List<string>>
        (
            c => c.FromFactory(() => fixture.CreateMany<string>().ToList())
        );

        return fixture;
    }
}