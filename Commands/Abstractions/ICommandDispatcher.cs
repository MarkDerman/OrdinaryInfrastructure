namespace Odin.Patterns.Commands;

/// <summary>
/// Dispatches commands to their registered command handlers.
/// </summary>
public interface ICommandDispatcher
{
    /// <summary>
    /// Dispatches a command that does not return a result
    /// to its registered command handler.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    /// <param name="command"></param>
    /// <param name="ct"></param>
    Task DispatchAsync<TCommand>(
        TCommand command,
        CancellationToken ct = default)
        where TCommand : ICommand;

    /// <summary>
    /// Dispatches a command that returns a result
    /// to its registered command handler.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    /// <typeparam name="TCommandResult"></typeparam>
    /// <param name="command"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<TCommandResult> DispatchAsync<TCommand, TCommandResult>(
        TCommand command,
        CancellationToken ct = default)
        where TCommand : ICommand<TCommandResult>;
}