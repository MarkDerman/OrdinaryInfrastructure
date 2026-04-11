using Microsoft.Extensions.DependencyInjection;
using Odin.DesignContracts;
using Odin.Logging;

namespace Odin.Patterns.Dispatching.Internal;

internal delegate void DispatcherLog<TDispatcher>(
    ILoggerWrapper<TDispatcher> logger,
    string message,
    object? firstArgument,
    object? secondArgument);

internal delegate void DispatcherExceptionLog<TDispatcher>(
    ILoggerWrapper<TDispatcher> logger,
    Exception exception,
    string message,
    object? firstArgument,
    object? secondArgument);

internal sealed class DispatcherRuntime<TDispatcher>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggerWrapper<TDispatcher> _logger;

    public DispatcherRuntime(
        IServiceProvider serviceProvider,
        ILoggerWrapper<TDispatcher> logger)
    {
        Precondition.RequiresNotNull(serviceProvider);
        Precondition.RequiresNotNull(logger);

        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public void LogDispatch(
        DispatcherLog<TDispatcher> logDispatch,
        string message,
        Type requestType,
        Type handlerInterfaceType)
    {
        logDispatch(
            _logger,
            message,
            FormatTypeName(requestType),
            FormatTypeName(handlerInterfaceType));
    }

    public THandler ResolveSingleHandler<THandler>(
        Type requestType,
        Type handlerInterfaceType,
        string requestKind)
        where THandler : class
    {
        THandler[] handlers = _serviceProvider.GetServices<THandler>().ToArray();

        if (handlers.Length == 1)
        {
            return handlers[0];
        }

        string message = handlers.Length == 0
            ? $"No {requestKind} handler was registered for {requestKind} type '{FormatTypeName(requestType)}'. " +
              $"Expected handler interface: '{FormatTypeName(handlerInterfaceType)}'."
            : $"Multiple {requestKind} handlers were registered for {requestKind} type '{FormatTypeName(requestType)}'. " +
              $"Expected handler interface: '{FormatTypeName(handlerInterfaceType)}'. Found {handlers.Length} registrations.";

        _logger.LogError(message);
        throw new InvalidOperationException(message);
    }

    public async Task ExecuteAsync(
        Func<Task> action,
        CancellationToken ct,
        Type requestType,
        Type implementationType,
        DispatcherLog<TDispatcher> logSuccess,
        DispatcherExceptionLog<TDispatcher> logCanceled,
        DispatcherExceptionLog<TDispatcher> logFailure,
        string successMessage,
        string canceledMessage,
        string failureMessage)
    {
        string requestTypeName = FormatTypeName(requestType);
        string implementationTypeName = FormatTypeName(implementationType);

        try
        {
            await action().ConfigureAwait(false);

            logSuccess(
                _logger,
                successMessage,
                requestTypeName,
                implementationTypeName);
        }
        catch (OperationCanceledException ex) when (ct.IsCancellationRequested)
        {
            logCanceled(
                _logger,
                ex,
                canceledMessage,
                requestTypeName,
                implementationTypeName);
            throw;
        }
        catch (Exception ex)
        {
            logFailure(
                _logger,
                ex,
                failureMessage,
                requestTypeName,
                implementationTypeName);
            throw;
        }
    }

    public async Task<TResult> ExecuteAsync<TResult>(
        Func<Task<TResult>> action,
        CancellationToken ct,
        Type requestType,
        Type implementationType,
        DispatcherLog<TDispatcher> logSuccess,
        DispatcherExceptionLog<TDispatcher> logCanceled,
        DispatcherExceptionLog<TDispatcher> logFailure,
        string successMessage,
        string canceledMessage,
        string failureMessage)
    {
        string requestTypeName = FormatTypeName(requestType);
        string implementationTypeName = FormatTypeName(implementationType);

        try
        {
            TResult result = await action().ConfigureAwait(false);

            logSuccess(
                _logger,
                successMessage,
                requestTypeName,
                implementationTypeName);

            return result;
        }
        catch (OperationCanceledException ex) when (ct.IsCancellationRequested)
        {
            logCanceled(
                _logger,
                ex,
                canceledMessage,
                requestTypeName,
                implementationTypeName);
            throw;
        }
        catch (Exception ex)
        {
            logFailure(
                _logger,
                ex,
                failureMessage,
                requestTypeName,
                implementationTypeName);
            throw;
        }
    }

    private static string FormatTypeName(Type type)
    {
        return type.FullName ?? type.Name;
    }
}
