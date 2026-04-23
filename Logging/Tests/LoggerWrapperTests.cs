using Microsoft.Extensions.Logging;
using Odin.Logging;

namespace Tests.Odin.Logging;

public sealed class LoggerWrapperTests
{
    [Fact]
    public void Constructor_requires_an_inner_logger()
    {
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() => new LoggerWrapper<TestCategory>(null!));

        Assert.Equal("logger", ex.ParamName);
    }

    [Fact]
    public void Log_generic_overload_forwards_the_original_state_and_formatter()
    {
        RecordingLogger<TestCategory> innerLogger = new();
        LoggerWrapper<TestCategory> sut = new(innerLogger);
        TestState state = new("Arthur");
        EventId eventId = new(42, "answer");
        InvalidOperationException exception = new("No tea");

        sut.Log(LogLevel.Information, eventId, state, exception, static (value, _) => $"Hello {value.Name}");

        LogEntry entry = Assert.Single(innerLogger.Entries);
        Assert.Equal(LogLevel.Information, entry.Level);
        Assert.Equal(eventId, entry.EventId);
        Assert.Same(state, entry.State);
        Assert.Same(exception, entry.Exception);
        Assert.Equal("Hello Arthur", entry.RenderedMessage);
    }

    [Theory]
    [MemberData(nameof(ExceptionOnlyLogCases))]
    public void Exception_only_overloads_forward_the_expected_log_level(Action<LoggerWrapper<TestCategory>, Exception> logAction, LogLevel expectedLevel)
    {
        RecordingLogger<TestCategory> innerLogger = new();
        LoggerWrapper<TestCategory> sut = new(innerLogger);
        InvalidOperationException exception = new("Pan Galactic Gargle Blaster");

        logAction(sut, exception);

        LogEntry entry = Assert.Single(innerLogger.Entries);
        Assert.Equal(expectedLevel, entry.Level);
        Assert.Equal(default, entry.EventId);
        Assert.Same(exception, entry.Exception);
    }

    [Fact]
    public void Convenience_message_overloads_preserve_event_id_exception_and_message_template()
    {
        RecordingLogger<TestCategory> innerLogger = new();
        LoggerWrapper<TestCategory> sut = new(innerLogger);
        EventId eventId = new(7, "warn");
        InvalidOperationException exception = new("Improbability drive offline");

        sut.LogWarning(eventId, exception, "Ship {ShipName} lost power at sector {Sector}", "Heart of Gold", 5);

        LogEntry entry = Assert.Single(innerLogger.Entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Equal(eventId, entry.EventId);
        Assert.Same(exception, entry.Exception);
        Assert.Equal("Ship Heart of Gold lost power at sector 5", entry.RenderedMessage);
        Assert.Equal("Ship {ShipName} lost power at sector {Sector}", entry.OriginalFormat);
    }

    [Fact]
    public void IsEnabled_returns_the_inner_logger_result()
    {
        RecordingLogger<TestCategory> innerLogger = new() { IsEnabledResult = false };
        LoggerWrapper<TestCategory> sut = new(innerLogger);

        bool result = sut.IsEnabled(LogLevel.Debug);

        Assert.False(result);
    }

    [Fact]
    public void BeginScope_generic_overload_forwards_the_original_state_and_scope()
    {
        RecordingLogger<TestCategory> innerLogger = new();
        LoggerWrapper<TestCategory> sut = new(innerLogger);
        TestState state = new("Ford");

        IDisposable? scope = sut.BeginScope(state);

        Assert.Same(innerLogger.Scope, scope);
        Assert.Same(state, Assert.Single(innerLogger.ScopeStates));
    }

    [Fact]
    public void BeginScope_message_template_overload_formats_the_scope_state()
    {
        RecordingLogger<TestCategory> innerLogger = new();
        LoggerWrapper<TestCategory> sut = new(innerLogger);

        IDisposable? scope = sut.BeginScope("Travelling with {Passenger}", "Trillian");

        object scopeState = Assert.Single(innerLogger.ScopeStates);
        Assert.Same(innerLogger.Scope, scope);
        Assert.Equal("Travelling with Trillian", scopeState.ToString());
        Assert.Equal("Travelling with {Passenger}", GetOriginalFormat(scopeState));
    }

    public static TheoryData<Action<LoggerWrapper<TestCategory>, Exception>, LogLevel> ExceptionOnlyLogCases()
    {
        return new TheoryData<Action<LoggerWrapper<TestCategory>, Exception>, LogLevel>
        {
            { static (logger, exception) => logger.Log(LogLevel.Trace, exception), LogLevel.Trace },
            { static (logger, exception) => logger.LogWarning(exception), LogLevel.Warning },
            { static (logger, exception) => logger.LogError(exception), LogLevel.Error },
            { static (logger, exception) => logger.LogCritical(exception), LogLevel.Critical }
        };
    }

    private static string? GetOriginalFormat(object state)
    {
        return state is IEnumerable<KeyValuePair<string, object?>> values
            ? values.FirstOrDefault(x => x.Key == "{OriginalFormat}").Value?.ToString()
            : null;
    }

    private sealed record TestState(string Name);

    public sealed class TestCategory;

    private sealed record LogEntry(
        LogLevel Level,
        EventId EventId,
        object? State,
        Exception? Exception,
        string RenderedMessage,
        string? OriginalFormat);

    private sealed class RecordingLogger<TCategoryName> : ILogger<TCategoryName>
    {
        public List<LogEntry> Entries { get; } = new();

        public List<object> ScopeStates { get; } = new();

        public bool IsEnabledResult { get; set; } = true;

        public TestScope Scope { get; } = new();

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
        {
            ScopeStates.Add(state);
            return Scope;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return IsEnabledResult;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add(new LogEntry(
                logLevel,
                eventId,
                state,
                exception,
                formatter(state, exception),
                state is IEnumerable<KeyValuePair<string, object?>> values
                    ? values.FirstOrDefault(x => x.Key == "{OriginalFormat}").Value?.ToString()
                    : null));
        }
    }

    private sealed class TestScope : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
