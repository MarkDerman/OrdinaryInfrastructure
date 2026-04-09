namespace Odin.Patterns.CommandHandler;

/// <summary>
/// Generic treatment of ICommandHandler for use in testing and other scenarios.
/// </summary>
/// <typeparam name="TCommand"></typeparam>
public class FakeCommandHandler<TCommand>  : ICommandHandler<TCommand> 
    where TCommand : ICommand
{
    /// <summary>
    /// Default constructor. 
    /// </summary>
    public FakeCommandHandler()
    {
        
    }
    
    /// <summary>
    /// Does nothing.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="ct"></param>
    /// <exception cref="NotImplementedException"></exception>
    public virtual async Task HandleAsync(TCommand command, CancellationToken ct = default)
    {
        await Task.CompletedTask;
    }
}

/// <summary>
/// Does not do anything.
/// </summary>
/// <typeparam name="TCommand"></typeparam>
/// <typeparam name="TResult"></typeparam>
public class FakeCommandHandler<TCommand, TResult>  : ICommandHandler<TCommand, TResult>  
    where TCommand : ICommand<TResult>
{
    private readonly TResult _resultToReturn;
    
    /// <summary>
    /// Initialise to return 'result' on HandleAsync.
    /// </summary>
    /// <param name="result"></param>
    public FakeCommandHandler(TResult result)
    {
        _resultToReturn = result;
    }
    
    /// <summary>
    /// Does nothing.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="ct"></param>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<TResult> HandleAsync(TCommand command, CancellationToken ct = default)
    {
        return await Task.FromResult(_resultToReturn);
    }
}