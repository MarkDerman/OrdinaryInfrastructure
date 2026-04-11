namespace Odin.Patterns.Queries;

/// <summary>
/// Defines a query request that returns
/// one or other form of data or a result.
/// </summary>
/// <typeparam name="TResult"></typeparam>
public interface IQuery<out TResult> { }
