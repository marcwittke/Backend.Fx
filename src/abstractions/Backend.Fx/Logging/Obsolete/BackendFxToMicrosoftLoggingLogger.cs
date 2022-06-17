using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

// ReSharper disable CheckNamespace
namespace Backend.Fx.Logging
{
    [Obsolete]
    public class BackendFxToMicrosoftLoggingLogger : ILogger
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public BackendFxToMicrosoftLoggingLogger(Microsoft.Extensions.Logging.ILogger logger)
        {
            _logger = logger;
        }

        public Exception Fatal(Exception exception)
        {
            _logger.LogCritical(exception, "Exception: {Message}", exception.Message);
            return exception;
        }

        public void Fatal([StructuredMessageTemplate] string format, params object[] args)
        {
            _logger.LogCritical(format, args);
        }

        public Exception Fatal(Exception exception, [StructuredMessageTemplate] string format, params object[] args)
        {
            _logger.LogCritical(exception, format, args);
            return exception;
        }
        
        

        public Exception Error(Exception exception)
        {
            _logger.LogError(exception, "Exception: {Message}", exception.Message);
            return exception;
        }

        public void Error([StructuredMessageTemplate] string format, params object[] args)
        {
            _logger.LogError(format, args);
        }

        public Exception Error(Exception exception, [StructuredMessageTemplate] string format, params object[] args)
        {
            _logger.LogError(exception, format, args);
            return exception;
        }
        
        
        public Exception Warn(Exception exception)
        {
            _logger.LogWarning(exception, "Exception: {Message}", exception.Message);
            return exception;
        }

        public void Warn([StructuredMessageTemplate] string format, params object[] args)
        {
            _logger.LogWarning(format, args);
        }

        public Exception Warn(Exception exception, [StructuredMessageTemplate] string format, params object[] args)
        {
            _logger.LogWarning(exception, format, args);
            return exception;
        }

        
        public Exception Info(Exception exception)
        {
            _logger.LogInformation(exception, "Exception: {Message}", exception.Message);
            return exception;
        }

        public IDisposable InfoDuration(string activity)
        {
            return new DurationLogger(s => _logger.LogInformation(s), activity);
        }

        public IDisposable InfoDuration(string beginMessage, string endMessage)
        {
            return new DurationLogger(s => _logger.LogInformation(s), beginMessage, endMessage);
        }

        public void Info([StructuredMessageTemplate] string format, params object[] args)
        {
            _logger.LogInformation(format, args);
        }

        public Exception Info(Exception exception, [StructuredMessageTemplate] string format, params object[] args)
        {
            _logger.LogInformation(exception, format, args);
            return exception;
        }

        public bool IsDebugEnabled()
        {
            return _logger.IsEnabled(LogLevel.Debug);
        }


        public Exception Debug(Exception exception)
        {
            _logger.LogDebug(exception, "Exception: {Message}", exception.Message);
            return exception;
        }

        public IDisposable DebugDuration(string activity)
        {
            return new DurationLogger(s => _logger.LogDebug(s), activity);
        }

        public IDisposable DebugDuration(string beginMessage, string endMessage)
        {
            return new DurationLogger(s => _logger.LogDebug(s), beginMessage, endMessage);
        }

        public void Debug([StructuredMessageTemplate] string format, params object[] args)
        {
            _logger.LogDebug(format, args);
        }

        public Exception Debug(Exception exception, [StructuredMessageTemplate] string format, params object[] args)
        {
            _logger.LogDebug(exception, format, args);
            return exception;
        }

        public bool IsTraceEnabled()
        {
            return _logger.IsEnabled(LogLevel.Trace);
        }


        public Exception Trace(Exception exception)
        {
            _logger.LogTrace(exception, "Exception: {Message}", exception.Message);
            return exception;
        }

        public IDisposable TraceDuration(string activity)
        {
            return new DurationLogger(s => _logger.LogTrace(s), activity);
        }

        public IDisposable TraceDuration(string beginMessage, string endMessage)
        {
            return new DurationLogger(s => _logger.LogTrace(s), beginMessage, endMessage);
        }

        public void Trace([StructuredMessageTemplate] string format, params object[] args)
        {
            _logger.LogTrace(format, args);
        }

        public Exception Trace(Exception exception, [StructuredMessageTemplate] string format, params object[] args)
        {
            _logger.LogTrace(exception, format, args);
            return exception;
        }
    }
}