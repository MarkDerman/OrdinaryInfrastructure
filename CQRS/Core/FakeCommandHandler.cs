namespace Odin.CQRS;

/// <summary>
/// Generic treatment of ICommandHandler for use in testing and other scenarios.
/// </summary>
/// <typeparam name="TCommand"></typeparam>
public class FakeCommandHandler<TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    private readonly Exception? _exceptionToThrow;
    
    /// <summary>
    /// The default constructor sets up HandleAsync to do nothing.
    /// </summary>
    public FakeCommandHandler()
    { }
    
    /// <summary>
    /// Initialise to throw an Exception on HandleAsync, unless
    /// errorToThrow is null.
    /// </summary>
    /// <param name="errorToThrow"></param>
    public FakeCommandHandler(Exception? errorToThrow)
    {
        _exceptionToThrow = errorToThrow;
    }
    
    /// <summary>
    /// Does nothing.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="ct"></param>
    /// <exception cref="NotImplementedException"></exception>
    public async Task HandleAsync(TCommand command, CancellationToken ct = default)
    {
        if (_exceptionToThrow != null)
            throw _exceptionToThrow;
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
    private readonly TResult? _resultToReturn;
    private readonly Exception? _exceptionToThrow;
    
    /// <summary>
    /// Initialise to return TResult on HandleAsync.
    /// </summary>
    /// <param name="result"></param>
    public FakeCommandHandler(TResult result)
    {
        _resultToReturn = result;
        _exceptionToThrow = null;
    }
    
    /// <summary>
    /// Initialise to throw an Exception on HandleAsync.
    /// </summary>
    /// <param name="errorToThrow"></param>
    public FakeCommandHandler(Exception errorToThrow)
    {
        _resultToReturn = default;
        _exceptionToThrow = errorToThrow;
    }
    
    /// <summary>
    /// Does nothing.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="ct"></param>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<TResult> HandleAsync(TCommand command, CancellationToken ct = default)
    {
        if (_exceptionToThrow != null)
            throw _exceptionToThrow;
        
        return await Task.FromResult(_resultToReturn!);
    }
}