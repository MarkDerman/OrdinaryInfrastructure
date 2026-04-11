using Microsoft.Extensions.DependencyInjection;
using Odin.DesignContracts;
using Odin.Logging;

namespace Odin.Patterns.Queries;

/// <summary>
/// Dispatches queries by resolving their matching query handler from an <see cref="IServiceProvider"/>.
/// </summary>
public sealed class ServiceProviderQueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggerWrapper<ServiceProviderQueryDispatcher> _logger;

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

        _serviceProvider = serviceProvider;
        _logger = logger;
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

        _logger.LogDebug(
            "Dispatching query {QueryType} using handler interface {HandlerInterface}.",
            FormatTypeName(queryType),
            FormatTypeName(handlerInterfaceType));

        IQueryHandler<TQuery, TQueryResult> handler =
            ResolveHandler<IQueryHandler<TQuery, TQueryResult>>(queryType, handlerInterfaceType);

        try
        {
            TQueryResult result = await handler.HandleAsync(query, ct).ConfigureAwait(false);

            _logger.LogDebug(
                "Query {QueryType} completed successfully using handler {HandlerType}.",
                FormatTypeName(queryType),
                FormatTypeName(handler.GetType()));

            return result;
        }
        catch (OperationCanceledException ex) when (ct.IsCancellationRequested)
        {
            _logger.LogDebug(
                ex,
                "Query {QueryType} was cancelled while executing handler {HandlerType}.",
                FormatTypeName(queryType),
                FormatTypeName(handler.GetType()));
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Query {QueryType} failed while executing handler {HandlerType}.",
                FormatTypeName(queryType),
                FormatTypeName(handler.GetType()));
            throw;
        }
    }

    private THandler ResolveHandler<THandler>(Type queryType, Type handlerInterfaceType)
        where THandler : class
    {
        THandler[] handlers = _serviceProvider.GetServices<THandler>().ToArray();

        if (handlers.Length == 1)
        {
            return handlers[0];
        }

        string message = handlers.Length == 0
            ? $"No query handler was registered for query type '{FormatTypeName(queryType)}'. " +
              $"Expected handler interface: '{FormatTypeName(handlerInterfaceType)}'."
            : $"Multiple query handlers were registered for query type '{FormatTypeName(queryType)}'. " +
              $"Expected handler interface: '{FormatTypeName(handlerInterfaceType)}'. Found {handlers.Length} registrations.";

        _logger.LogError(message);
        throw new InvalidOperationException(message);
    }

    private static string FormatTypeName(Type type)
    {
        return type.FullName ?? type.Name;
    }
}
