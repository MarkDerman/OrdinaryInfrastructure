using Microsoft.Extensions.Logging;

namespace Odin.System;

/// <summary>
/// Structured message payload for result objects that need
/// severity and optional exception details.
/// </summary>
public class ResultMessage
{
    /// <summary>
    /// Message content. Can be null when <see cref="Error"/> is supplied.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Message severity.
    /// </summary>
    public LogLevel Severity { get; init; } = LogLevel.Information;

    /// <summary>
    /// Exception, if present.
    /// </summary>
    public Exception? Error { get; init; }

    /// <summary>
    /// Creates an informational message.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="severity"></param>
    /// <returns></returns>
    public static ResultMessage Info(string message, LogLevel severity = LogLevel.Information)
    {
        return new ResultMessage { Message = message, Severity = severity };
    }

    /// <summary>
    /// Creates an error message.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="error"></param>
    /// <param name="severity"></param>
    /// <returns></returns>
    public static ResultMessage Failure(string? message, Exception? error = null, LogLevel severity = LogLevel.Error)
    {
        Precondition.Requires(!string.IsNullOrWhiteSpace(message) || error != null, "Either a message or an error is required.");
        return new ResultMessage { Message = message, Severity = severity, Error = error };
    }

    /// <summary>
    /// Outputs 'Severity: Message Error.Message'
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{Severity}:{(Message != null ? " " + Message : null)}{(Error != null ? " " + Error.Message : null)}";
    }
}
