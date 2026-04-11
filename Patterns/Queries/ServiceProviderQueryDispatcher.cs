using Odin.DesignContracts;
using Odin.Logging;
using Odin.Patterns.Dispatching.Internal;

namespace Odin.Patterns.Queries;

/// <summary>
/// Dispatches queries by resolving their matching query handler from an <see cref="IServiceProvider"/>.
/// </summary>
public sealed class ServiceProviderQueryDispatcher : IQueryDispatcher
{
    private static readonly DispatcherLog<ServiceProviderQueryDispatcher> DebugLog =
        static (logger, message, firstArgument, secondArgument) => logger.LogDebug(message, firstArgument, secondArgument);

    private static readonly DispatcherExceptionLog<ServiceProviderQueryDispatcher> DebugExceptionLog =
        static (logger, exception, message, firstArgument, secondArgument) =>
            logger.LogDebug(exception, message, firstArgument, secondArgument);

    private static readonly DispatcherExceptionLog<ServiceProviderQueryDispatcher> ErrorExceptionLog =
        static (logger, exception, message, firstArgument, secondArgument) =>
            logger.LogError(exception, message, firstArgument, secondArgument);

    private readonly DispatcherRuntime<ServiceProviderQueryDispatcher> _runtime;

    /// <summary>
    /// Creates a new dispatcher.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve query handlers.</param>
    /// <param name="logger">The logger used to record dispatch activity.</param>
    public ServiceProviderQueryDispatcher(
        IServiceProvider serviceProvider,
        ILoggerWrapper<ServiceProviderQueryDispatcher> logger)
    {
        Precondition.RequiresNotNull(serviceProvider);
        Precondition.RequiresNotNull(logger);

        _runtime = new DispatcherRuntime<ServiceProviderQueryDispatcher>(serviceProvider, logger);
    }

    /// <inheritdoc />
    public async Task<TQueryResult> DispatchAsync<TQuery, TQueryResult>(
        TQuery query,
        CancellationToken ct = default)
        where TQuery : IQuery<TQueryResult>
    {
        Precondition.RequiresNotNull(query);

        Type queryType = typeof(TQuery);
        Type handlerInterfaceType = typeof(IQueryHandler<TQuery, TQueryResult>);

        _runtime.LogDispatch(
            DebugLog,
            "Dispatching query {QueryType} using handler interface {HandlerInterface}.",
            queryType,
            handlerInterfaceType);

        IQueryHandler<TQuery, TQueryResult> handler =
            _runtime.ResolveSingleHandler<IQueryHandler<TQuery, TQueryResult>>(queryType, handlerInterfaceType, "query");

        return await _runtime.ExecuteAsync(
            () => handler.HandleAsync(query, ct),
            ct,
            queryType,
            handler.GetType(),
            DebugLog,
            DebugExceptionLog,
            ErrorExceptionLog,
            "Query {QueryType} completed successfully using handler {HandlerType}.",
            "Query {QueryType} was cancelled while executing handler {HandlerType}.",
            "Query {QueryType} failed while executing handler {HandlerType}.").ConfigureAwait(false);
    }
}
