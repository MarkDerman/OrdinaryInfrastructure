using Odin.Patterns.Queries;

namespace Tests.Odin.Patterns.Queries;

// ReSharper disable once ClassNeverInstantiated.Local
internal sealed record TestQuery(int Value) : IQuery<string>;

// ReSharper disable once ClassNeverInstantiated.Local
internal sealed record AlternateTestQuery(string Value) : IQuery<int>;

internal sealed class TestQueryHandler : IQueryHandler<TestQuery, string>
{
    public Task<string> HandleAsync(TestQuery query, CancellationToken ct = default)
    {
        return Task.FromResult(query.Value.ToString());
    }
}

internal sealed class AlternateTestQueryHandler : IQueryHandler<AlternateTestQuery, int>
{
    public Task<int> HandleAsync(AlternateTestQuery query, CancellationToken ct = default)
    {
        return Task.FromResult(query.Value.Length);
    }
}
