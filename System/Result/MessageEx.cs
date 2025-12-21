using Microsoft.Extensions.Logging;

namespace Odin.System;

/// <summary>
/// Extended 'Message' including a Severity, and optional Exception. 
/// </summary>
public record MessageEx
{
    /// <summary>
    /// Message content. Can be null.
    /// </summary>
    public string? Message { get; init; }
    
    /// <summary>
    /// Message severity: Error, Warning, Information, etc...
    /// </summary>
    public LogLevel Severity { get; init; } = LogLevel.Information;
    
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