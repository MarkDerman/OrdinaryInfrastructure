using Moq;
using Odin.Logging;
using Odin.Patterns.Queries;

namespace Tests.Odin.Patterns.Queries;

[TestFixture]
public sealed class ServiceProviderQueryDispatcherTests
{
    [Test]
    public async Task DispatchAsync_for_query_calls_resolved_handler_and_logs_success()
    {
        RecordingQueryHandler handler = new();
        Mock<ILoggerWrapper<ServiceProviderQueryDispatcher>> loggerMock = CreateLoggerMock();
        ServiceProviderQueryDispatcher sut = new(
            new TestServiceProvider().AddHandlers<IQueryHandler<TestQuery, string>>(handler),
            loggerMock.Object);

        TestQuery query = new(42);

        string result = await sut.DispatchAsync<TestQuery, string>(query);

        Assert.That(result, Is.EqualTo("42"));
        Assert.That(handler.ReceivedQuery, Is.SameAs(query));
        loggerMock.Verify(
            x => x.LogDebug(
                "Dispatching query {QueryType} using handler interface {HandlerInterface}.",
                It.Is<object?[]>(args =>
                    args.Length == 2 &&
                    Equals(args[0], typeof(TestQuery).FullName))),
            Times.Once);
        loggerMock.Verify(
            x => x.LogDebug(
                "Query {QueryType} completed successfully using handler {HandlerType}.",
                It.Is<object?[]>(args =>
                    args.Length == 2 &&
                    Equals(args[0], typeof(TestQuery).FullName))),
            Times.Once);
    }

    [Test]
    public void DispatchAsync_when_no_handler_is_registered_throws_with_query_and_handler_details()
    {
        Mock<ILoggerWrapper<ServiceProviderQueryDispatcher>> loggerMock = CreateLoggerMock();
        ServiceProviderQueryDispatcher sut = new(new TestServiceProvider(), loggerMock.Object);

        InvalidOperationException? ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await sut.DispatchAsync<TestQuery, string>(new TestQuery(7)));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.Message, Does.Contain(typeof(TestQuery).FullName));
        Assert.That(ex.Message, Does.Contain(typeof(IQueryHandler<TestQuery, string>).FullName));
        loggerMock.Verify(
            x => x.LogError(
                It.Is<string?>(message =>
                    message != null &&
                    message.Contains(typeof(TestQuery).FullName!) &&
                    message.Contains(typeof(IQueryHandler<TestQuery, string>).FullName!)),
                It.IsAny<object?[]>()),
            Times.Once);
    }

    [Test]
    public void DispatchAsync_when_multiple_handlers_are_registered_throws_with_query_and_handler_details()
    {
        Mock<ILoggerWrapper<ServiceProviderQueryDispatcher>> loggerMock = CreateLoggerMock();
        ServiceProviderQueryDispatcher sut = new(
            new TestServiceProvider().AddHandlers<IQueryHandler<TestQuery, string>>(
                new RecordingQueryHandler(),
                new RecordingQueryHandler()),
            loggerMock.Object);

        InvalidOperationException? ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await sut.DispatchAsync<TestQuery, string>(new TestQuery(7)));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.Message, Does.Contain(typeof(TestQuery).FullName));
        Assert.That(ex.Message, Does.Contain(typeof(IQueryHandler<TestQuery, string>).FullName));
        Assert.That(ex.Message, Does.Contain("Found 2 registrations"));
    }

    [Test]
    public void DispatchAsync_bubbles_handler_exceptions_and_logs_error()
    {
        InvalidOperationException expectedException = new("Kaboom");
        ThrowingQueryHandler handler = new(expectedException);
        Mock<ILoggerWrapper<ServiceProviderQueryDispatcher>> loggerMock = CreateLoggerMock();
        ServiceProviderQueryDispatcher sut = new(
            new TestServiceProvider().AddHandlers<IQueryHandler<TestQuery, string>>(handler),
            loggerMock.Object);

        InvalidOperationException? ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await sut.DispatchAsync<TestQuery, string>(new TestQuery(1)));

        Assert.That(ex, Is.SameAs(expectedException));
        loggerMock.Verify(
            x => x.LogError(
                expectedException,
                "Query {QueryType} failed while executing handler {HandlerType}.",
                It.Is<object?[]>(args =>
                    args.Length == 2 &&
                    Equals(args[0], typeof(TestQuery).FullName))),
            Times.Once);
    }

    [Test]
    public void DispatchAsync_passes_the_cancellation_token_to_the_handler()
    {
        CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        CancellableQueryHandler handler = new();
        Mock<ILoggerWrapper<ServiceProviderQueryDispatcher>> loggerMock = CreateLoggerMock();
        ServiceProviderQueryDispatcher sut = new(
            new TestServiceProvider().AddHandlers<IQueryHandler<TestQuery, string>>(handler),
            loggerMock.Object);

        OperationCanceledException? ex = Assert.ThrowsAsync<OperationCanceledException>(
            async () => await sut.DispatchAsync<TestQuery, string>(new TestQuery(3), cancellationTokenSource.Token));

        Assert.That(ex, Is.Not.Null);
        Assert.That(handler.ReceivedCancellationToken, Is.EqualTo(cancellationTokenSource.Token));
    }

    private static Mock<ILoggerWrapper<ServiceProviderQueryDispatcher>> CreateLoggerMock()
    {
        Mock<ILoggerWrapper<ServiceProviderQueryDispatcher>> loggerMock = new();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<Microsoft.Extensions.Logging.LogLevel>())).Returns(true);
        return loggerMock;
    }

    private sealed class RecordingQueryHandler : IQueryHandler<TestQuery, string>
    {
        public TestQuery? ReceivedQuery { get; private set; }

        public Task<string> HandleAsync(TestQuery query, CancellationToken ct = default)
        {
            ReceivedQuery = query;
            return Task.FromResult(query.Value.ToString());
        }
    }

    private sealed class ThrowingQueryHandler(Exception exceptionToThrow) : IQueryHandler<TestQuery, string>
    {
        public Task<string> HandleAsync(TestQuery query, CancellationToken ct = default)
        {
            throw exceptionToThrow;
        }
    }

    private sealed class CancellableQueryHandler : IQueryHandler<TestQuery, string>
    {
        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<string> HandleAsync(TestQuery query, CancellationToken ct = default)
        {
            ReceivedCancellationToken = ct;
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(query.Value.ToString());
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
