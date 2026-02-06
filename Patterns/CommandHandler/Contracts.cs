namespace Odin.Patterns;

/// <summary>
/// Defines a command request that doesn't return a value
/// </summary>
public interface ICommand { }

/// <summary>
///  Defines a command request that returns an operation result
/// (e.g., a new ID, a Result class, etc.)
/// or could be a query that returns query results data. 
/// </summary>
/// <typeparam name="TResult"></typeparam>
public interface ICommand<out TResult> { }

/// <summary>
/// Defines the handling implementation for a command request that does not return a Result.
/// </summary>
/// <typeparam name="TCommand"></typeparam>
public interface ICommandHandler<in TCommand> 
    where TCommand : ICommand
{
    Task HandleAsync(TCommand command, CancellationToken ct = default);
}

/// <summary>
/// Defines the handling implementation for a command request that returns a Result.
/// </summary>
/// <typeparam name="TCommand"></typeparam>
/// <typeparam name="TResult"></typeparam>
public interface ICommandHandler<in TCommand, TResult> 
    where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken ct = default);
}