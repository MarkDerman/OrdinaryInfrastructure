// Portions of this file are derived from the .NET runtime project.
// Copyright (c) .NET Foundation and Contributors
// Licensed under the MIT license.
// See the LICENSE.md file in this repository for full license information.

using Microsoft.Extensions.Logging;

namespace Odin.Logging
{
    /// <summary>
    /// Provides an expanded ILogger and ILogger of T interface for LogXXX(a, b, c, ...) calls 
    /// on the same basis as the .NET LoggerExtensions extension methods,
    /// that is much more convenient for asserting logger calls compared to mocking ILogger, 
    /// and asserting ILogger -> Log(LogLevel logLevel, EventId eventId, TState state, Exception? exception, etc...
    /// Odin = OrDinary INfrastructure.
    /// </summary>
    /// <typeparam name="TCategoryName"></typeparam>
    public interface ILoggerWrapper<out TCategoryName> : ILoggerWrapper
    {
    }

    /// <summary>
    /// Logging adapter interface to wrap ILogger mainly to ease mocking in tests
    /// </summary>
    public interface ILoggerWrapper : ILogger
    {
        /// <summary>
        /// Logs an Exception
        /// </summary>
        /// <param name="level"></param>
        /// <param name="exception"></param>
        void Log(LogLevel level, Exception exception);

        /// <summary>
        /// Logs a Warning with an Exception
        /// </summary>
        /// <param name="exception"></param>
        void LogWarning(Exception exception);
        
        /// <summary>
        /// Logs an Error with an Exception
        /// </summary>
        /// <param name="exception"></param>
        void LogError(Exception exception);

        /// <summary>
        /// Logs a Critical with an Exception
        /// </summary>
        /// <param name="exception"></param>
        void LogCritical(Exception exception);
        
        
        //----------INTERFACES AS PER Microsoft.Extensions.Logging.LoggerExtensions ---------------//

        
        //------------------------------------------DEBUG------------------------------------------//

        /// <summary>
        /// Formats and writes a debug log message.
        /// </summary>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogDebug(0, exception, "Error while processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogDebug(EventId eventId, Exception? exception, string? message, params object?[] args);

        /// <summary>
        /// Formats and writes a debug log message.
        /// </summary>
        
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogDebug(0, "Processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogDebug(EventId eventId, string? message, params object?[] args);


        /// <summary>
        /// Formats and writes a debug log message.
        /// </summary>
        
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogDebug(exception, "Error while processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogDebug(Exception? exception, string? message, params object?[] args);


        /// <summary>
        /// Formats and writes a debug log message.
        /// </summary>
        
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogDebug("Processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogDebug(string? message, params object?[] args);


        //------------------------------------------TRACE------------------------------------------//

        /// <summary>
        /// Formats and writes a trace log message.
        /// </summary>

        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogTrace(0, exception, "Error while processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogTrace(EventId eventId, Exception? exception, string? message, params object?[] args);

        /// <summary>
        /// Formats and writes a trace log message.
        /// </summary>

        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogTrace(0, "Processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogTrace(EventId eventId, string? message, params object?[] args);


        /// <summary>
        /// Formats and writes a trace log message.
        /// </summary>

        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogTrace(exception, "Error while processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogTrace(Exception? exception, string? message, params object?[] args);

        /// <summary>
        /// Formats and writes a trace log message.
        /// </summary>

        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogTrace("Processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogTrace(string? message, params object?[] args);


        //------------------------------------------INFORMATION------------------------------------------//

        /// <summary>
        /// Formats and writes an informational log message.
        /// </summary>

        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogInformation(0, exception, "Error while processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogInformation(EventId eventId, Exception? exception, string? message, params object?[] args);


        /// <summary>
        /// Formats and writes an informational log message.
        /// </summary>

        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogInformation(0, "Processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogInformation(EventId eventId, string? message, params object?[] args);


        /// <summary>
        /// Formats and writes an informational log message.
        /// </summary>

        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogInformation(exception, "Error while processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogInformation(Exception? exception, string? message, params object?[] args);

        /// <summary>
        /// Formats and writes an informational log message.
        /// </summary>

        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogInformation("Processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogInformation(string? message, params object?[] args);


        //------------------------------------------WARNING------------------------------------------//

        /// <summary>
        /// Formats and writes a warning log message.
        /// </summary>

        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogWarning(0, exception, "Error while processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogWarning(EventId eventId, Exception? exception, string? message, params object?[] args);

        /// <summary>
        /// Formats and writes a warning log message.
        /// </summary>

        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogWarning(0, "Processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogWarning(EventId eventId, string? message, params object?[] args);


        /// <summary>
        /// Formats and writes a warning log message.
        /// </summary>

        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogWarning(exception, "Error while processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogWarning(Exception? exception, string? message, params object?[] args);


        /// <summary>
        /// Formats and writes a warning log message.
        /// </summary>

        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogWarning("Processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogWarning(string? message, params object?[] args);


        //------------------------------------------ERROR------------------------------------------//

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary>

        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogError(0, exception, "Error while processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogError(EventId eventId, Exception? exception, string? message, params object?[] args);


        /// <summary>
        /// Formats and writes an error log message.
        /// </summary>

        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogError(0, "Processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogError(EventId eventId, string? message, params object?[] args);


        /// <summary>
        /// Formats and writes an error log message.
        /// </summary>

        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogError(exception, "Error while processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogError(Exception? exception, string? message, params object?[] args);

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary>

        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogError("Processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogError(string? message, params object?[] args);


        //------------------------------------------CRITICAL------------------------------------------//

        /// <summary>
        /// Formats and writes a critical log message.
        /// </summary>

        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogCritical(0, exception, "Error while processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogCritical(EventId eventId, Exception? exception, string? message, params object?[] args);

        /// <summary>
        /// Formats and writes a critical log message.
        /// </summary>

        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogCritical(0, "Processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogCritical(EventId eventId, string? message, params object?[] args);

        /// <summary>
        /// Formats and writes a critical log message.
        /// </summary>

        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogCritical(exception, "Error while processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogCritical(Exception? exception, string? message, params object?[] args);

        /// <summary>
        /// Formats and writes a critical log message.
        /// </summary>

        /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <example>
        /// <code language="csharp">
        /// logger.LogCritical("Processing request from {Address}", address)
        /// </code>
        /// </example>
        public void LogCritical(string? message, params object?[] args);

        /// <summary>
        /// Formats and writes a log message at the specified log level.
        /// </summary>

        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Log(LogLevel logLevel, string? message, params object?[] args);

        /// <summary>
        /// Formats and writes a log message at the specified log level.
        /// </summary>

        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Log(LogLevel logLevel, EventId eventId, string? message, params object?[] args);

        /// <summary>
        /// Formats and writes a log message at the specified log level.
        /// </summary>

        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Log(LogLevel logLevel, Exception? exception, string? message, params object?[] args);

        /// <summary>
        /// Formats and writes a log message at the specified log level.
        /// </summary>

        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="eventId">The event id associated with the log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Log(LogLevel logLevel, EventId eventId, Exception? exception, string? message, params object?[] args);

        //------------------------------------------Scope------------------------------------------//

        /// <summary>
        /// Formats the message and creates a scope.
        /// </summary>
        /// <param name="messageFormat">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c>.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>A disposable scope object. Can be null.</returns>
        /// <example>
        /// <code language="csharp">
        /// using(logger.BeginScope("Processing request from {Address}", address)) { }
        /// </code>
        /// </example>
        public IDisposable? BeginScope(
            string messageFormat,
            params object?[] args);

    }
}
