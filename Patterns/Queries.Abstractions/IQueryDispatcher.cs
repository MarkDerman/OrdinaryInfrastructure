namespace Odin.Patterns.Queries;

/// <summary>
/// Dispatches queries to their registered query handlers.
/// </summary>
public interface IQueryDispatcher
{
    /// <summary>
    /// Dispatches a query that returns a result
    /// to its registered query handler.
    /// </summary>
    /// <typeparam name="TQuery"></typeparam>
    /// <typeparam name="TQueryResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<TQueryResult> DispatchAsync<TQuery, TQueryResult>(
        TQuery query,
        CancellationToken ct = default)
        where TQuery : IQuery<TQueryResult>;
}
