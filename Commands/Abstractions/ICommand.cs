namespace Odin.Patterns.Commands;

/// <summary>
///  Defines a command request that returns an operation result
/// (e.g., a new ID, a Result class, etc.)
/// or could be a query that returns query results data. 
/// </summary>
/// <typeparam name="TResult"></typeparam>
public interface ICommand<out TResult> { }

/// <summary>
/// Defines a command request that doesn't return a value
/// </summary>
public interface ICommand
{
}