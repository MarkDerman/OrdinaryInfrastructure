using Microsoft.Extensions.Logging;

namespace Tests.Odin.System;

public record MessageError(string Message, Exception? Error);

public record MessageLoggingInfo(LogLevel Severity, string Message, Exception? Error);

public class MessageSeverity(LogLevel Severity, string Message);

