namespace Odin.DesignContracts.Rewriter;

/// <summary>
/// This is to avoid needing to ship Microsoft.Build.Framework assemblies
/// with the tooling if we want to use MSBuild LogImportance directly.
/// </summary>
public enum LogImportance
{
    /// <summary>
    /// High importance messages
    /// </summary>
    High,
    /// <summary>
    /// Normal importance messages
    /// </summary>
    Normal,
    /// <summary>
    /// Low importance messages
    /// </summary>
    Low
}

/// <summary>
/// Created so that AssemblyRewriter can log to console or MSBuild output.
/// </summary>
public interface ILoggingAdaptor
{
    /// <summary>
    /// Logs a message of the given importance using the specified string.
    /// Thread safe.
    /// </summary>
    /// <remarks>
    /// Take care to order the parameters correctly or the other overload will be called inadvertently.
    /// </remarks>
    /// <param name="importance">Log verbosity level of the message.</param>
    /// <param name="message">The message string.</param>
    /// <param name="messageArgs">Optional arguments for formatting the message string.</param>
    /// <exception cref="ArgumentNullException">Thrown when <c>message</c> is null.</exception>
    public void LogMessage(LogImportance importance, string? message, params object[] messageArgs);


    /// <summary>
    /// Logs a message using the specified string and other message details.
    /// Thread safe.
    /// </summary>
    /// <param name="subcategory">Description of the warning type (can be null).</param>
    /// <param name="code">Message code (can be null)</param>
    /// <param name="helpKeyword">The help keyword for the host IDE (can be null).</param>
    /// <param name="file">The path to the file causing the message (can be null).</param>
    /// <param name="lineNumber">The line in the file causing the message (set to zero if not available).</param>
    /// <param name="columnNumber">The column in the file causing the message (set to zero if not available).</param>
    /// <param name="endLineNumber">The last line of a range of lines in the file causing the message (set to zero if not available).</param>
    /// <param name="endColumnNumber">The last column of a range of columns in the file causing the message (set to zero if not available).</param>
    /// <param name="importance">Importance of the message.</param>
    /// <param name="message">The message string.</param>
    /// <param name="messageArgs">Optional arguments for formatting the message string.</param>
    /// <exception cref="ArgumentNullException">Thrown when <c>message</c> is null.</exception>
    public void LogMessage(
        string? subcategory,
        string? code,
        string? helpKeyword,
        string? file,
        int lineNumber,
        int columnNumber,
        int endLineNumber,
        int endColumnNumber,
        LogImportance importance,
        string? message,
        params object[] messageArgs);

    /// <summary>
    /// Logs an error using the message, and optionally the stack-trace from the given exception, and
    /// optionally inner exceptions too.
    /// Thread safe.
    /// </summary>
    /// <param name="exception">Exception to log.</param>
    /// <param name="showStackTrace">If true, callstack will be appended to message.</param>
    /// <param name="showDetail">Whether to log exception types and any inner exceptions.</param>
    /// <param name="file">File related to the exception, or null if the project file should be logged</param>
    /// <exception cref="ArgumentNullException">Thrown when <c>exception</c> is null.</exception>
    public void LogErrorFromException(Exception exception, bool showStackTrace, bool showDetail, string file);
}