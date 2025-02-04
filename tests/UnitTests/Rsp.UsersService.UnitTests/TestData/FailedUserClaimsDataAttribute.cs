using AutoFixture;
using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Identity;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.WebApi.Requests;

namespace Rsp.UsersService.UnitTests.TestData;

[AttributeUsage(AttributeTargets.Method)]
public class FailedUserClaimsDataAttribute : AutoDataAttribute
{
    public FailedUserClaimsDataAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<IrasUser>(c => c
            .With(u => u.Email, fixture.Create<string>()));

        fixture.Customize<UserClaimsRequest>(c => c
            .With(r => r.Email, fixture.Create<string>())
            .With(r => r.Claims, fixture.CreateMany<KeyValuePair<string, string>>().ToList()));

        fixture.Customize<IdentityError>(c => c
            .With(e => e.Code, fixture.Create<string>())
            .With(e => e.Description, fixture.Create<string>()));

        return fixture;
    }
}
