using Moq;
using Odin.Logging;
using Odin.Patterns.Commands;
using Xunit;

namespace Tests.Odin.Patterns.Commands;

public sealed class ServiceProviderCommandDispatcherTests
{
    [Fact]
    public async Task DispatchAsync_for_void_command_calls_resolved_handler_and_logs_success()
    {
        RecordingCommandHandler handler = new RecordingCommandHandler();
        Mock<ILoggerWrapper<ServiceProviderCommandDispatcher>> loggerMock = CreateLoggerMock();
        ServiceProviderCommandDispatcher sut = new(
            new TestServiceProvider().AddHandlers<ICommandHandler<TestCommand>>(handler),
            loggerMock.Object);

        TestCommand command = new("Arthur");

        await sut.DispatchAsync(command);

        Assert.Same(command, handler.ReceivedCommand);
        loggerMock.Verify(
            x => x.LogTrace(
                "Dispatching command {CommandType} using handler interface {HandlerInterface}.",
                It.Is<object?[]>(args =>
                    args.Length == 2 &&
                    Equals(args[0], typeof(TestCommand).FullName))),
            Times.Once);
        loggerMock.Verify(
            x => x.LogTrace(
                "Command {CommandType} completed successfully using handler {HandlerType}.",
                It.Is<object?[]>(args =>
                    args.Length == 2 &&
                    Equals(args[0], typeof(TestCommand).FullName))),
            Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_for_result_command_returns_handler_result()
    {
        ResultCommandHandler handler = new("Trillian");
        Mock<ILoggerWrapper<ServiceProviderCommandDispatcher>> loggerMock = CreateLoggerMock();
        ServiceProviderCommandDispatcher sut = new(
            new TestServiceProvider().AddHandlers<ICommandHandler<ResultCommand, string>>(handler),
            loggerMock.Object);

        string result = await sut.DispatchAsync<ResultCommand, string>(new ResultCommand(42));

        Assert.Equal("Trillian", result);
    }

    [Fact]
    public async Task DispatchAsync_when_no_handler_is_registered_throws_with_command_and_handler_details()
    {
        Mock<ILoggerWrapper<ServiceProviderCommandDispatcher>> loggerMock = CreateLoggerMock();
        ServiceProviderCommandDispatcher sut = new(new TestServiceProvider(), loggerMock.Object);

        InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.DispatchAsync(new TestCommand("Ford")));

        Assert.Contains(typeof(TestCommand).FullName, ex.Message);
        Assert.Contains(typeof(ICommandHandler<TestCommand>).FullName, ex.Message);
        loggerMock.Verify(
            x => x.LogError(
                It.Is<string?>(message =>
                    message != null &&
                    message.Contains(typeof(TestCommand).FullName!) &&
                    message.Contains(typeof(ICommandHandler<TestCommand>).FullName!)),
                It.IsAny<object?[]>()),
            Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_when_multiple_handlers_are_registered_throws_with_command_and_handler_details()
    {
        Mock<ILoggerWrapper<ServiceProviderCommandDispatcher>> loggerMock = CreateLoggerMock();
        ServiceProviderCommandDispatcher sut = new(
            new TestServiceProvider().AddHandlers<ICommandHandler<TestCommand>>(
                new RecordingCommandHandler(),
                new RecordingCommandHandler()),
            loggerMock.Object);

        InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.DispatchAsync(new TestCommand("Ford")));

        Assert.Contains(typeof(TestCommand).FullName, ex.Message);
        Assert.Contains(typeof(ICommandHandler<TestCommand>).FullName, ex.Message);
        Assert.Contains("Found 2 registrations", ex.Message);
    }

    [Fact]
    public async Task DispatchAsync_bubbles_handler_exceptions_and_logs_error()
    {
        InvalidOperationException expectedException = new("Kaboom");
        ThrowingCommandHandler handler = new(expectedException);
        Mock<ILoggerWrapper<ServiceProviderCommandDispatcher>> loggerMock = CreateLoggerMock();
        ServiceProviderCommandDispatcher sut = new(
            new TestServiceProvider().AddHandlers<ICommandHandler<TestCommand>>(handler),
            loggerMock.Object);

        InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.DispatchAsync(new TestCommand("Zaphod")));

        Assert.Same(expectedException, ex);
        loggerMock.Verify(
            x => x.LogError(
                expectedException,
                "Command {CommandType} failed while executing handler {HandlerType}.",
                It.Is<object?[]>(args =>
                    args.Length == 2 &&
                    Equals(args[0], typeof(TestCommand).FullName))),
            Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_passes_the_cancellation_token_to_the_handler()
    {
        CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        CancellableCommandHandler handler = new();
        Mock<ILoggerWrapper<ServiceProviderCommandDispatcher>> loggerMock = CreateLoggerMock();
        ServiceProviderCommandDispatcher sut = new(
            new TestServiceProvider().AddHandlers<ICommandHandler<TestCommand>>(handler),
            loggerMock.Object);

        OperationCanceledException ex = await Assert.ThrowsAsync<OperationCanceledException>(
            () => sut.DispatchAsync(new TestCommand("Marvin"), cancellationTokenSource.Token));

        Assert.Equal(cancellationTokenSource.Token, handler.ReceivedCancellationToken);
    }

    private static Mock<ILoggerWrapper<ServiceProviderCommandDispatcher>> CreateLoggerMock()
    {
        Mock<ILoggerWrapper<ServiceProviderCommandDispatcher>> loggerMock = new();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<Microsoft.Extensions.Logging.LogLevel>())).Returns(true);
        return loggerMock;
    }

    private sealed record TestCommand(string Value) : ICommand;

    private sealed record ResultCommand(int Value) : ICommand<string>;

    private sealed class RecordingCommandHandler : ICommandHandler<TestCommand>
    {
        public TestCommand? ReceivedCommand { get; private set; }

        public Task HandleAsync(TestCommand command, CancellationToken ct = default)
        {
            ReceivedCommand = command;
            return Task.CompletedTask;
        }
    }

    private sealed class ResultCommandHandler(string valueToReturn) : ICommandHandler<ResultCommand, string>
    {
        public Task<string> HandleAsync(ResultCommand command, CancellationToken ct = default)
        {
            return Task.FromResult(valueToReturn);
        }
    }

    private sealed class ThrowingCommandHandler(Exception exceptionToThrow) : ICommandHandler<TestCommand>
    {
        public Task HandleAsync(TestCommand command, CancellationToken ct = default)
        {
            throw exceptionToThrow;
        }
    }

    private sealed class CancellableCommandHandler : ICommandHandler<TestCommand>
    {
        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task HandleAsync(TestCommand command, CancellationToken ct = default)
        {
            ReceivedCancellationToken = ct;
            ct.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }

    private sealed class TestServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object?> _services = new();

        public TestServiceProvider AddHandlers<THandler>(params THandler[] handlers)
            where THandler : class
        {
            _services[typeof(IEnumerable<THandler>)] = handlers;
            return this;
        }

        public object? GetService(Type serviceType)
        {
            if (_services.TryGetValue(serviceType, out object? service))
            {
                return service;
            }

            if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                Type elementType = serviceType.GetGenericArguments()[0];
                return Array.CreateInstance(elementType, 0);
            }

            return null;
        }
    }
}
