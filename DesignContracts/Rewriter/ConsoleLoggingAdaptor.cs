using Microsoft.Build.Framework;

namespace Odin.DesignContracts.Rewriter;

/// <inheritdoc />
public class ConsoleLoggingAdaptor : ILoggingAdaptor
{
    /// <inheritdoc />
    public void LogMessage(LogImportance importance, string? message, params object[] messageArgs)
    {
        Console.WriteLine(message, messageArgs);
        if (importance == LogImportance.High)
        {
            Console.Error.WriteLine(message, messageArgs);
        }
        else if (importance == LogImportance.Normal)
        {
            Console.WriteLine(message, messageArgs);
        }
        // Don't write Low diagnostic messages to the console
    }

    /// <inheritdoc />
    public void LogMessage(string? subcategory, string? code, string? helpKeyword, string? file, int lineNumber, int columnNumber,
        int endLineNumber, int endColumnNumber, LogImportance importance, string? message, params object[] messageArgs)
    {
        LogMessage(importance, message, messageArgs);
    }

    /// <inheritdoc />
    public void LogErrorFromException(Exception exception, bool showStackTrace, bool showDetail, string file)
    {
        Console.WriteLine(exception.Message);

    }
}