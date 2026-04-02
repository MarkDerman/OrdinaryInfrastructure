// Portions of this file are derived from the .NET runtime project.
// Copyright (c) .NET Foundation and Contributors
// Licensed under the MIT license. See the LICENSE.md file in this repository for full license information.

using Microsoft.Extensions.Logging;

namespace Odin.Logging
{
    /// <inheritdoc/>
    public class LoggerWrapper<TCategoryName> : ILoggerWrapper<TCategoryName>
    {
        private readonly ILogger<TCategoryName> _logger;

        /// <summary>
        /// Constructor wraps ILogger of T
        /// </summary>
        /// <param name="logger"></param>
        public LoggerWrapper(ILogger<TCategoryName> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        //---------- Convenience Exception only logging -----------//

        /// <inheritdoc/>
        public void Log(LogLevel level, Exception exception)
        {
            Log(level, 0, exception, null);
        }
        
        /// <inheritdoc/>
        public void LogWarning(Exception exception)
        {
            Log(LogLevel.Warning, 0, exception,null);
        }

        /// <inheritdoc/>
        public void LogError(Exception exception)
        {
            Log(LogLevel.Error, 0, exception, null);
        }

        /// <inheritdoc/>
        public void LogCritical(Exception exception)
        {
            Log(LogLevel.Critical, 0, exception,null);
        }
        
        //---------- Wrap ILogger access itself as well ---------------//

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _logger.Log(logLevel, eventId, state, exception, formatter);
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        /// <inheritdoc/>
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return _logger.BeginScope<TState>(state);
        }
        
        //-----Support convenience extensions as per Microsoft.Extensions.Logging.LoggerExtensions --------//
        
        /// <inheritdoc/>
        public void Log(LogLevel logLevel, string? message, params object?[] args)
        {
            Log(logLevel, 0, null, message, args);
        }

        /// <inheritdoc/>
        public void Log(LogLevel logLevel, EventId eventId, string? message, params object?[] args)
        {
            Log(logLevel, eventId, null, message, args);
        }

        /// <inheritdoc/>
        public void Log(LogLevel logLevel, Exception? exception, string? message = null, params object?[] args)
        {
            // if (string.IsNullOrWhiteSpace(message) && exception != null)
            // {
            //     message = exception.Message;
            // }
            Log(logLevel, 0, exception, message, args);
        }

        /// <inheritdoc/>
        public void Log(LogLevel logLevel, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            // Use the innermost ILogger Log call from LoggerExtensions
            _logger.Log(logLevel, eventId, exception, message, args);
        }
        
        //------------------------------------------DEBUG------------------------------------------//

        /// <inheritdoc/>
        public void LogDebug(EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            Log(LogLevel.Debug, eventId, exception, message, args);
        }

        /// <inheritdoc/>
        public void LogDebug(EventId eventId, string? message, params object?[] args)
        {
            Log(LogLevel.Debug, eventId, message, args);
        }

        /// <inheritdoc/>
        public void LogDebug(Exception? exception, string? message, params object?[] args)
        {
            Log(LogLevel.Debug, exception, message, args);
        }

        /// <inheritdoc/>
        public void LogDebug(string? message, params object?[] args)
        {
            Log(LogLevel.Debug, message, args);
        }

        //------------------------------------------TRACE------------------------------------------//

        /// <inheritdoc/>
        public void LogTrace(EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            Log(LogLevel.Trace, eventId, exception, message, args);
        }

        /// <inheritdoc/>
        public void LogTrace(EventId eventId, string? message, params object?[] args)
        {
            Log(LogLevel.Trace, eventId, message, args);
        }

        /// <inheritdoc/>
        public void LogTrace(Exception? exception, string? message, params object?[] args)
        {
            Log(LogLevel.Trace, exception, message, args);
        }

        /// <inheritdoc/>
        public void LogTrace(string? message, params object?[] args)
        {
            Log(LogLevel.Trace, message, args);
        }

        //------------------------------------------INFORMATION------------------------------------------//

        /// <inheritdoc/>
        public void LogInformation(EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            Log(LogLevel.Information, eventId, exception, message, args);
        }

        /// <inheritdoc/>
        public void LogInformation(EventId eventId, string? message, params object?[] args)
        {
            Log(LogLevel.Information, eventId, message, args);
        }

        /// <inheritdoc/>
        public void LogInformation(Exception? exception, string? message, params object?[] args)
        {
            Log(LogLevel.Information, exception, message, args);
        }

        /// <inheritdoc/>
        public void LogInformation(string? message, params object?[] args)
        {
            Log(LogLevel.Information, message, args);
        }

        //------------------------------------------WARNING------------------------------------------//

        /// <inheritdoc/>
        public void LogWarning(EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            Log(LogLevel.Warning, eventId, exception, message, args);
        }

        /// <inheritdoc/>
        public void LogWarning(EventId eventId, string? message, params object?[] args)
        {
            Log(LogLevel.Warning, eventId, message, args);
        }

        /// <inheritdoc/>
        public void LogWarning(Exception? exception, string? message, params object?[] args)
        {
            Log(LogLevel.Warning, exception, message, args);
        }

        /// <inheritdoc/>
        public void LogWarning(string? message, params object?[] args)
        {
            Log(LogLevel.Warning, message, args);
        }

        //------------------------------------------ERROR------------------------------------------//

        /// <inheritdoc/>
        public void LogError(EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            Log(LogLevel.Error, eventId, exception, message, args);
        }

        /// <inheritdoc/>
        public void LogError(EventId eventId, string? message, params object?[] args)
        {
            Log(LogLevel.Error, eventId, message, args);
        }

        /// <inheritdoc/>
        public void LogError(Exception? exception, string? message = null, params object?[] args)
        {
            Log(LogLevel.Error, exception, message, args);
        }

        /// <inheritdoc/>
        public void LogError(string? message, params object?[] args)
        {
            Log(LogLevel.Error, message, args);
        }

        //------------------------------------------CRITICAL------------------------------------------//

        /// <inheritdoc/>
        public void LogCritical(EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            Log(LogLevel.Critical, eventId, exception, message, args);
        }

        /// <inheritdoc/>
        public void LogCritical(EventId eventId, string? message, params object?[] args)
        {
            Log(LogLevel.Critical, eventId, message, args);
        }

        /// <inheritdoc/>
        public void LogCritical(Exception? exception, string? message, params object?[] args)
        {
            Log(LogLevel.Critical, exception, message, args);
        }

        /// <inheritdoc/>
        public void LogCritical(string? message, params object?[] args)
        {
            Log(LogLevel.Critical, message, args);
        }

        //------------------------------------------Scope------------------------------------------//

        /// <inheritdoc/>
        public IDisposable? BeginScope(string messageFormat, params object?[] args)
        {
            return _logger.BeginScope(messageFormat, args);
        }
        
        
        
    }
}
