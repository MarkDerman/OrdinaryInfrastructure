using Microsoft.Extensions.Logging;

namespace Odin.System;

/// <summary>
/// Perfect for logging
/// </summary>
public record ResultMessage2
{
    /// <summary>
    /// Message severity: Error, Warning, Information, etc...
    /// </summary>
    public LogLevel Severity { get; init; } = LogLevel.Information;
    
    /// <summary>
    /// Message content.
    /// </summary>
    public string? Message { get; init; }
    
    /// <summary>
    /// Exception, if present.
    /// </summary>
    public Exception? Error { get; set; }

    /// <summary>
    /// Outputs 'Severity: Message Error.Message'
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{Severity}:{(Message!=null ? " " + Message : null)}{(Error!=null ? " " + Error.Message : null)}";
    }
}