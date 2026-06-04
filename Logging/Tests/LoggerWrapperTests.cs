using Microsoft.Extensions.Logging;
using Odin.Logging;

namespace Tests.Odin.Logging;

public sealed class LoggerWrapperTests
{
    [Test]
    public void Constructor_requires_an_inner_logger()
    {
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() => new LoggerWrapper<TestCategory>(null!));

        Assert.That(ex.ParamName, Is.EqualTo("logger"));
    }

    [Test]
    public void Log_generic_overload_forwards_the_original_state_and_formatter()
    {
        RecordingLogger<TestCategory> innerLogger = new();
        LoggerWrapper<TestCategory> sut = new(innerLogger);
        TestState state = new("Arthur");
        EventId eventId = new(42, "answer");
        InvalidOperationException exception = new("No tea");

        sut.Log(LogLevel.Information, eventId, state, exception, static (value, _) => $"Hello {value.Name}");

        LogEntry entry = innerLogger.Entries.Single();
        Assert.That(entry.Level, Is.EqualTo(LogLevel.Information));
        Assert.That(entry.EventId, Is.EqualTo(eventId));
        Assert.That(entry.State, Is.SameAs(state));
        Assert.That(entry.Exception, Is.SameAs(exception));
        Assert.That(entry.RenderedMessage, Is.EqualTo("Hello Arthur"));
    }
    [TestCaseSource(nameof(ExceptionOnlyLogCases))]
    public void Exception_only_overloads_forward_the_expected_log_level(Action<LoggerWrapper<TestCategory>, Exception> logAction, LogLevel expectedLevel)
    {
        RecordingLogger<TestCategory> innerLogger = new();
        LoggerWrapper<TestCategory> sut = new(innerLogger);
        InvalidOperationException exception = new("Pan Galactic Gargle Blaster");

        logAction(sut, exception);

        LogEntry entry = innerLogger.Entries.Single();
        Assert.That(entry.Level, Is.EqualTo(expectedLevel));
        Assert.That(entry.EventId, Is.EqualTo(default(EventId)));
        Assert.That(entry.Exception, Is.SameAs(exception));
    }

    [Test]
    public void Convenience_message_overloads_preserve_event_id_exception_and_message_template()
    {
        RecordingLogger<TestCategory> innerLogger = new();
        LoggerWrapper<TestCategory> sut = new(innerLogger);
        EventId eventId = new(7, "warn");
        InvalidOperationException exception = new("Improbability drive offline");

        sut.LogWarning(eventId, exception, "Ship {ShipName} lost power at sector {Sector}", "Heart of Gold", 5);

        LogEntry entry = innerLogger.Entries.Single();
        Assert.That(entry.Level, Is.EqualTo(LogLevel.Warning));
        Assert.That(entry.EventId, Is.EqualTo(eventId));
        Assert.That(entry.Exception, Is.SameAs(exception));
        Assert.That(entry.RenderedMessage, Is.EqualTo("Ship Heart of Gold lost power at sector 5"));
        Assert.That(entry.OriginalFormat, Is.EqualTo("Ship {ShipName} lost power at sector {Sector}"));
    }

    [Test]
    public void IsEnabled_returns_the_inner_logger_result()
    {
        RecordingLogger<TestCategory> innerLogger = new() { IsEnabledResult = false };
        LoggerWrapper<TestCategory> sut = new(innerLogger);

        bool result = sut.IsEnabled(LogLevel.Debug);

        Assert.That(result, Is.False);
    }

    [Test]
    public void BeginScope_generic_overload_forwards_the_original_state_and_scope()
    {
        RecordingLogger<TestCategory> innerLogger = new();
        LoggerWrapper<TestCategory> sut = new(innerLogger);
        TestState state = new("Ford");

        IDisposable? scope = sut.BeginScope(state);

        Assert.That(scope, Is.SameAs(innerLogger.Scope));
        Assert.That(innerLogger.ScopeStates.Single(), Is.SameAs(state));
    }

    [Test]
    public void BeginScope_message_template_overload_formats_the_scope_state()
    {
        RecordingLogger<TestCategory> innerLogger = new();
        LoggerWrapper<TestCategory> sut = new(innerLogger);

        IDisposable? scope = sut.BeginScope("Travelling with {Passenger}", "Trillian");

        object scopeState = innerLogger.ScopeStates.Single();
        Assert.That(scope, Is.SameAs(innerLogger.Scope));
        Assert.That(scopeState.ToString(), Is.EqualTo("Travelling with Trillian"));
        Assert.That(GetOriginalFormat(scopeState), Is.EqualTo("Travelling with {Passenger}"));
    }

    public static IEnumerable<TestCaseData> ExceptionOnlyLogCases()
    {
        yield return new TestCaseData(
            new Action<LoggerWrapper<TestCategory>, Exception>(static (logger, exception) => logger.Log(LogLevel.Trace, exception)),
            LogLevel.Trace);
        yield return new TestCaseData(
            new Action<LoggerWrapper<TestCategory>, Exception>(static (logger, exception) => logger.LogWarning(exception)),
            LogLevel.Warning);
        yield return new TestCaseData(
            new Action<LoggerWrapper<TestCategory>, Exception>(static (logger, exception) => logger.LogError(exception)),
            LogLevel.Error);
        yield return new TestCaseData(
            new Action<LoggerWrapper<TestCategory>, Exception>(static (logger, exception) => logger.LogCritical(exception)),
            LogLevel.Critical);
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
