using Microsoft.Extensions.DependencyInjection;
using Odin.DesignContracts;
using Odin.Logging;

namespace Odin.Patterns.Commands;

/// <summary>
/// Dispatches commands by resolving their matching command handler from an <see cref="IServiceProvider"/>.
/// </summary>
public sealed class ServiceProviderCommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggerWrapper<ServiceProviderCommandDispatcher> _logger;

    /// <summary>
    /// Creates a new dispatcher.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve command handlers.</param>
    /// <param name="logger">The logger used to record dispatch activity.</param>
    public ServiceProviderCommandDispatcher(
        IServiceProvider serviceProvider,
        ILoggerWrapper<ServiceProviderCommandDispatcher> logger)
    {
        Contract.RequiresNotNull(serviceProvider);
        Contract.RequiresNotNull(logger);

        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task DispatchAsync<TCommand>(
        TCommand command,
        CancellationToken ct = default)
        where TCommand : ICommand
    {
        Contract.RequiresNotNull(command);

        Type commandType = typeof(TCommand);
        Type handlerInterfaceType = typeof(ICommandHandler<TCommand>);

        _logger.LogTrace(
            "Dispatching command {CommandType} using handler interface {HandlerInterface}.",
            FormatTypeName(commandType),
            FormatTypeName(handlerInterfaceType));

        ICommandHandler<TCommand> handler = ResolveHandler<ICommandHandler<TCommand>>(commandType, handlerInterfaceType);

        try
        {
            await handler.HandleAsync(command, ct).ConfigureAwait(false);

            _logger.LogTrace(
                "Command {CommandType} completed successfully using handler {HandlerType}.",
                FormatTypeName(commandType),
                FormatTypeName(handler.GetType()));
        }
        catch (OperationCanceledException ex) when (ct.IsCancellationRequested)
        {
            _logger.LogDebug(
                ex,
                "Command {CommandType} was cancelled while executing handler {HandlerType}.",
                FormatTypeName(commandType),
                FormatTypeName(handler.GetType()));
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Command {CommandType} failed while executing handler {HandlerType}.",
                FormatTypeName(commandType),
                FormatTypeName(handler.GetType()));
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<TCommandResult> DispatchAsync<TCommand, TCommandResult>(
        TCommand command,
        CancellationToken ct = default)
        where TCommand : ICommand<TCommandResult>
    {
        Contract.RequiresNotNull(command);

        Type commandType = typeof(TCommand);
        Type handlerInterfaceType = typeof(ICommandHandler<TCommand, TCommandResult>);

        _logger.LogDebug(
            "Dispatching command {CommandType} using handler interface {HandlerInterface}.",
            FormatTypeName(commandType),
            FormatTypeName(handlerInterfaceType));

        ICommandHandler<TCommand, TCommandResult> handler =
            ResolveHandler<ICommandHandler<TCommand, TCommandResult>>(commandType, handlerInterfaceType);

        try
        {
            TCommandResult result = await handler.HandleAsync(command, ct).ConfigureAwait(false);

            _logger.LogDebug(
                "Command {CommandType} completed successfully using handler {HandlerType}.",
                FormatTypeName(commandType),
                FormatTypeName(handler.GetType()));

            return result;
        }
        catch (OperationCanceledException ex) when (ct.IsCancellationRequested)
        {
            _logger.LogDebug(
                ex,
                "Command {CommandType} was cancelled while executing handler {HandlerType}.",
                FormatTypeName(commandType),
                FormatTypeName(handler.GetType()));
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Command {CommandType} failed while executing handler {HandlerType}.",
                FormatTypeName(commandType),
                FormatTypeName(handler.GetType()));
            throw;
        }
    }

    private THandler ResolveHandler<THandler>(Type commandType, Type handlerInterfaceType)
        where THandler : class
    {
        THandler[] handlers = _serviceProvider.GetServices<THandler>().ToArray();

        if (handlers.Length == 1)
        {
            return handlers[0];
        }

        string message = handlers.Length == 0
            ? $"No command handler was registered for command type '{FormatTypeName(commandType)}'. " +
              $"Expected handler interface: '{FormatTypeName(handlerInterfaceType)}'."
            : $"Multiple command handlers were registered for command type '{FormatTypeName(commandType)}'. " +
              $"Expected handler interface: '{FormatTypeName(handlerInterfaceType)}'. Found {handlers.Length} registrations.";

        _logger.LogError(message);
        throw new InvalidOperationException(message);
    }

    private static string FormatTypeName(Type type)
    {
        return type.FullName ?? type.Name;
    }
}
