using Odin.DesignContracts;
using Odin.Logging;
using Odin.Patterns.Dispatching.Internal;

namespace Odin.Patterns.Commands;

/// <summary>
/// Dispatches commands by resolving their matching command handler from an <see cref="IServiceProvider"/>.
/// </summary>
public sealed class ServiceProviderCommandDispatcher : ICommandDispatcher
{
    private static readonly DispatcherLog<ServiceProviderCommandDispatcher> TraceLog =
        static (logger, message, firstArgument, secondArgument) => logger.LogTrace(message, firstArgument, secondArgument);

    private static readonly DispatcherLog<ServiceProviderCommandDispatcher> DebugLog =
        static (logger, message, firstArgument, secondArgument) => logger.LogDebug(message, firstArgument, secondArgument);

    private static readonly DispatcherExceptionLog<ServiceProviderCommandDispatcher> DebugExceptionLog =
        static (logger, exception, message, firstArgument, secondArgument) =>
            logger.LogDebug(exception, message, firstArgument, secondArgument);

    private static readonly DispatcherExceptionLog<ServiceProviderCommandDispatcher> ErrorExceptionLog =
        static (logger, exception, message, firstArgument, secondArgument) =>
            logger.LogError(exception, message, firstArgument, secondArgument);

    private readonly DispatcherRuntime<ServiceProviderCommandDispatcher> _runtime;

    /// <summary>
    /// Creates a new dispatcher.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve command handlers.</param>
    /// <param name="logger">The logger used to record dispatch activity.</param>
    public ServiceProviderCommandDispatcher(
        IServiceProvider serviceProvider,
        ILoggerWrapper<ServiceProviderCommandDispatcher> logger)
    {
        Precondition.RequiresNotNull(serviceProvider);
        Precondition.RequiresNotNull(logger);

        _runtime = new DispatcherRuntime<ServiceProviderCommandDispatcher>(serviceProvider, logger);
    }

    /// <inheritdoc />
    public async Task DispatchAsync<TCommand>(
        TCommand command,
        CancellationToken ct = default)
        where TCommand : ICommand
    {
        Precondition.RequiresNotNull(command);

        Type commandType = typeof(TCommand);
        Type handlerInterfaceType = typeof(ICommandHandler<TCommand>);

        _runtime.LogDispatch(
            TraceLog,
            "Dispatching command {CommandType} using handler interface {HandlerInterface}.",
            commandType,
            handlerInterfaceType);

        ICommandHandler<TCommand> handler =
            _runtime.ResolveSingleHandler<ICommandHandler<TCommand>>(commandType, handlerInterfaceType, "command");

        await _runtime.ExecuteAsync(
            () => handler.HandleAsync(command, ct),
            ct,
            commandType,
            handler.GetType(),
            TraceLog,
            DebugExceptionLog,
            ErrorExceptionLog,
            "Command {CommandType} completed successfully using handler {HandlerType}.",
            "Command {CommandType} was cancelled while executing handler {HandlerType}.",
            "Command {CommandType} failed while executing handler {HandlerType}.").ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<TCommandResult> DispatchAsync<TCommand, TCommandResult>(
        TCommand command,
        CancellationToken ct = default)
        where TCommand : ICommand<TCommandResult>
    {
        Precondition.RequiresNotNull(command);

        Type commandType = typeof(TCommand);
        Type handlerInterfaceType = typeof(ICommandHandler<TCommand, TCommandResult>);

        _runtime.LogDispatch(
            DebugLog,
            "Dispatching command {CommandType} using handler interface {HandlerInterface}.",
            commandType,
            handlerInterfaceType);

        ICommandHandler<TCommand, TCommandResult> handler =
            _runtime.ResolveSingleHandler<ICommandHandler<TCommand, TCommandResult>>(commandType, handlerInterfaceType, "command");

        return await _runtime.ExecuteAsync(
            () => handler.HandleAsync(command, ct),
            ct,
            commandType,
            handler.GetType(),
            DebugLog,
            DebugExceptionLog,
            ErrorExceptionLog,
            "Command {CommandType} completed successfully using handler {HandlerType}.",
            "Command {CommandType} was cancelled while executing handler {HandlerType}.",
            "Command {CommandType} failed while executing handler {HandlerType}.").ConfigureAwait(false);
    }
}
