using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

// ReSharper disable CheckNamespace
namespace Backend.Fx.Logging
{
    [Obsolete]
    public static class LoggerExtensions
    {
        public static Exception Fatal(this Microsoft.Extensions.Logging.ILogger logger, Exception exception)
        {
            logger.LogCritical(exception, "Exception: {Message}", exception.Message);
            return exception;
        }

        public static void Fatal(this Microsoft.Extensions.Logging.ILogger logger, [StructuredMessageTemplate] string format, params object[] args)
        {
            logger.LogCritical(format, args);
        }

        public static Exception Fatal(this Microsoft.Extensions.Logging.ILogger logger, Exception exception, [StructuredMessageTemplate] string format, params object[] args)
        {
            logger.LogCritical(exception, format, args);
            return exception;
        }
        
        

        public static Exception Error(this Microsoft.Extensions.Logging.ILogger logger, Exception exception)
        {
            logger.LogError(exception, "Exception: {Message}", exception.Message);
            return exception;
        }

        public static void Error(this Microsoft.Extensions.Logging.ILogger logger, [StructuredMessageTemplate] string format, params object[] args)
        {
            logger.LogError(format, args);
        }

        public static Exception Error(this Microsoft.Extensions.Logging.ILogger logger, Exception exception, [StructuredMessageTemplate] string format, params object[] args)
        {
            logger.LogError(exception, format, args);
            return exception;
        }
        
        
        public static Exception Warn(this Microsoft.Extensions.Logging.ILogger logger, Exception exception)
        {
            logger.LogWarning(exception, "Exception: {Message}", exception.Message);
            return exception;
        }

        public static void Warn(this Microsoft.Extensions.Logging.ILogger logger, [StructuredMessageTemplate] string format, params object[] args)
        {
            logger.LogWarning(format, args);
        }

        public static Exception Warn(this Microsoft.Extensions.Logging.ILogger logger, Exception exception, [StructuredMessageTemplate] string format, params object[] args)
        {
            logger.LogWarning(exception, format, args);
            return exception;
        }

        
        public static Exception Info(this Microsoft.Extensions.Logging.ILogger logger, Exception exception)
        {
            logger.LogInformation(exception, "Exception: {Message}", exception.Message);
            return exception;
        }

        public static IDisposable InfoDuration(this Microsoft.Extensions.Logging.ILogger logger, string activity)
        {
            return new DurationLogger(s => logger.LogInformation(s), activity);
        }

        public static IDisposable InfoDuration(this Microsoft.Extensions.Logging.ILogger logger, string beginMessage, string endMessage)
        {
            return new DurationLogger(s => logger.LogInformation(s), beginMessage, endMessage);
        }

        public static void Info(this Microsoft.Extensions.Logging.ILogger logger, [StructuredMessageTemplate] string format, params object[] args)
        {
            logger.LogInformation(format, args);
        }

        public static Exception Info(this Microsoft.Extensions.Logging.ILogger logger, Exception exception, [StructuredMessageTemplate] string format, params object[] args)
        {
            logger.LogInformation(exception, format, args);
            return exception;
        }

        public static bool IsDebugEnabled(this Microsoft.Extensions.Logging.ILogger logger)
        {
            return logger.IsEnabled(LogLevel.Debug);
        }


        public static Exception Debug(this Microsoft.Extensions.Logging.ILogger logger, Exception exception)
        {
            logger.LogDebug(exception, "Exception: {Message}", exception.Message);
            return exception;
        }

        public static IDisposable DebugDuration(this Microsoft.Extensions.Logging.ILogger logger, string activity)
        {
            return new DurationLogger(s => logger.LogDebug(s), activity);
        }

        public static IDisposable DebugDuration(this Microsoft.Extensions.Logging.ILogger logger, string beginMessage, string endMessage)
        {
            return new DurationLogger(s => logger.LogDebug(s), beginMessage, endMessage);
        }

        public static void Debug(this Microsoft.Extensions.Logging.ILogger logger, [StructuredMessageTemplate] string format, params object[] args)
        {
            logger.LogDebug(format, args);
        }

        public static Exception Debug(this Microsoft.Extensions.Logging.ILogger logger, Exception exception, [StructuredMessageTemplate] string format, params object[] args)
        {
            logger.LogDebug(exception, format, args);
            return exception;
        }

        public static bool IsTraceEnabled(this Microsoft.Extensions.Logging.ILogger logger)
        {
            return logger.IsEnabled(LogLevel.Trace);
        }


        public static Exception Trace(this Microsoft.Extensions.Logging.ILogger logger, Exception exception)
        {
            logger.LogTrace(exception, "Exception: {Message}", exception.Message);
            return exception;
        }

        public static IDisposable TraceDuration(this Microsoft.Extensions.Logging.ILogger logger, string activity)
        {
            return new DurationLogger(s => logger.LogTrace(s), activity);
        }

        public static IDisposable TraceDuration(this Microsoft.Extensions.Logging.ILogger logger, string beginMessage, string endMessage)
        {
            return new DurationLogger(s => logger.LogTrace(s), beginMessage, endMessage);
        }

        public static void Trace(this Microsoft.Extensions.Logging.ILogger logger, [StructuredMessageTemplate] string format, params object[] args)
        {
            logger.LogTrace(format, args);
        }

        public static Exception Trace(this Microsoft.Extensions.Logging.ILogger logger, Exception exception, [StructuredMessageTemplate] string format, params object[] args)
        {
            logger.LogTrace(exception, format, args);
            return exception;
        }
    }
}