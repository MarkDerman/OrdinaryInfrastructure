namespace Odin.Patterns.Commands;

/// <summary>
/// Defines the handling implementation for a command request that does not return a Result.
/// </summary>
/// <typeparam name="TCommand"></typeparam>
public interface ICommandHandler<in TCommand> 
    where TCommand : ICommand
{
    /// <summary>
    /// Handles the command request
    /// </summary>
    /// <param name="command"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task HandleAsync(TCommand command, CancellationToken ct = default);
}

/// <summary>
/// Defines the handling implementation for a command request that returns a Result.
/// </summary>
/// <typeparam name="TCommand"></typeparam>
/// <typeparam name="TCommandResult"></typeparam>
public interface ICommandHandler<in TCommand, TCommandResult> 
    where TCommand : ICommand<TCommandResult>
{
    /// <summary>
    /// Handles the command request and returns a result.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<TCommandResult> HandleAsync(TCommand command, CancellationToken ct = default);
}