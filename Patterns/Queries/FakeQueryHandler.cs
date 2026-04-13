namespace Odin.Patterns.Queries;

/// <summary>
/// Returns the result passed in via the constructor.
/// </summary>
/// <typeparam name="TQuery"></typeparam>
/// <typeparam name="TResult"></typeparam>
public class FakeQueryHandler<TQuery, TResult>  : IQueryHandler<TQuery, TResult>  
    where TQuery : IQuery<TResult>
{
    private readonly TResult _resultToReturn;
    
    /// <summary>
    /// Initialise to return 'result' on HandleAsync.
    /// </summary>
    /// <param name="result"></param>
    public FakeQueryHandler(TResult result)
    {
        _resultToReturn = result;
    }
    
    /// <summary>
    /// Does nothing.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="ct"></param>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<TResult> HandleAsync(TQuery query, CancellationToken ct = default)
    {
        return await Task.FromResult(_resultToReturn);
    }
}