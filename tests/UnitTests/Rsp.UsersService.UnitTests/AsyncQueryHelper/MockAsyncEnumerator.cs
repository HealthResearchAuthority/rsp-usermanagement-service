namespace Rsp.UsersService.UnitTests.AsyncQueryHelper;

public class MockAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public MockAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync() =>
        ValueTask.FromResult(_inner.MoveNext());

    public T Current => _inner.Current;
}
