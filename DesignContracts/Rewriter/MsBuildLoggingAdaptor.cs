using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Odin.DesignContracts.Rewriter;

/// <inheritdoc />
public class MsBuildLoggingAdaptor : ILoggingAdaptor
{
    private readonly TaskLoggingHelper _msBuildLogger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="msBuildLogger"></param>
    public MsBuildLoggingAdaptor(TaskLoggingHelper msBuildLogger)
    {
        _msBuildLogger = msBuildLogger;
    }

    /// <inheritdoc />
    public void LogMessage(LogImportance importance, string? message, params object[] messageArgs)
    {
        _msBuildLogger.LogMessage(MapImportance(importance), message, messageArgs);
    }

    /// <inheritdoc />
    public void LogMessage(string? subcategory, string? code, string? helpKeyword, string? file, int lineNumber, int columnNumber,
        int endLineNumber, int endColumnNumber, LogImportance importance, string? message, params object[] messageArgs)
    {
        _msBuildLogger.LogMessage(subcategory, code, helpKeyword, file, lineNumber, columnNumber, endLineNumber,
            endColumnNumber, MapImportance(importance), message, messageArgs);
    }

    private MessageImportance MapImportance(LogImportance importance)
    {
        switch (importance)
        {
            case LogImportance.High:
                return MessageImportance.High;
            case LogImportance.Normal:
                return MessageImportance.Normal;
            case LogImportance.Low:
                return MessageImportance.Low;
            default:
                throw new ArgumentOutOfRangeException(nameof(importance), importance, null);
        }
    }

    /// <inheritdoc />
    public void LogErrorFromException(Exception exception, bool showStackTrace, bool showDetail, string file)
    {
        _msBuildLogger.LogErrorFromException(exception, showStackTrace, showDetail, file);
    }
}