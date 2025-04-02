using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Rsp.UsersService.UnitTests.AsyncQueryHelper;

public class MockAsyncQueryProvider<T>(IQueryProvider inner) : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner = inner;

    public IQueryable CreateQuery(Expression expression) => new MockAsyncEnumerable<T>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
        new MockAsyncEnumerable<TElement>(expression);

    public object? Execute(Expression expression) => _inner.Execute(expression);

    public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var method = typeof(Task).GetMethod(nameof(Task.FromResult))
            ?? throw new InvalidOperationException($"Could not find method {nameof(Task.FromResult)} on type {typeof(Task)}.");

        var resultType = typeof(TResult).GetGenericArguments()[0];
        var genericMethod = method.MakeGenericMethod(resultType);

        var result = _inner.Execute(expression);
        var taskInstance = genericMethod.Invoke(null, [result])
            ?? throw new InvalidOperationException("Failed to create Task instance.");

        return (TResult)taskInstance;
    }
}