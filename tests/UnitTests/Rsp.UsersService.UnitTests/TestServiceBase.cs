using Moq.AutoMock;

namespace Rsp.UsersService.UnitTests;

public class TestServiceBase
{
    public AutoMocker Mocker { get; }

    public TestServiceBase()
    {
        Mocker = new AutoMocker();
    }
}