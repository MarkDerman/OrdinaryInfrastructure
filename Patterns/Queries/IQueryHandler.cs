namespace Odin.Patterns.Queries;

/// <summary>
/// Defines the handling implementation for a command request that returns a Result.
/// </summary>
/// <typeparam name="TQuery"></typeparam>
/// <typeparam name="TQueryResult"></typeparam>
public interface IQueryHandler<in TQuery, TQueryResult> 
    where TQuery : IQuery<TQueryResult>
{
    /// <summary>
    /// Handles the query request and returns a result.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<TQueryResult> HandleAsync(TQuery query, CancellationToken ct = default);
}