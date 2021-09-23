using System;
using System.Diagnostics;
using Backend.Fx.Logging;
using Serilog.Core;
using Serilog.Events;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace Backend.Fx.SerilogLogging
{
    [DebuggerStepThrough]
    public class SerilogLogger : ILogger
    {
        private readonly Serilog.ILogger _logger;

        internal SerilogLogger(Serilog.ILogger logger)
        {
            _logger = logger;
        }

        #region fatal

        public Exception Fatal(Exception exception)
        {
            _logger.Fatal(exception, string.Empty);
            return exception;
        }

        [MessageTemplateFormatMethod("format")]
        public void Fatal(string format, params object[] args)
        {
            _logger.Fatal(format, args);
        }

        [MessageTemplateFormatMethod("format")]
        public Exception Fatal(Exception exception, string format, params object[] args)
        {
            _logger.Fatal(exception, format, args);
            return exception;
        }

        #endregion

        #region error

        public Exception Error(Exception exception)
        {
            _logger.Error(exception, string.Empty);
            return exception;
        }

        [MessageTemplateFormatMethod("format")]
        public void Error(string format, params object[] args)
        {
            _logger.Error(format, args);
        }

        [MessageTemplateFormatMethod("format")]
        public Exception Error(Exception exception, string format, params object[] args)
        {
            _logger.Error(exception, format, args);
            return exception;
        }

        #endregion

        #region warn

        public Exception Warn(Exception exception)
        {
            _logger.Warning(exception, string.Empty);
            return exception;
        }

        [MessageTemplateFormatMethod("format")]
        public void Warn(string format, params object[] args)
        {
            _logger.Warning(format, args);
        }

        [MessageTemplateFormatMethod("format")]
        public Exception Warn(Exception exception, string format, params object[] args)
        {
            _logger.Warning(exception, format, args);
            return exception;
        }

        #endregion

        #region info

        public Exception Info(Exception exception)
        {
            _logger.Information(exception, string.Empty);
            return exception;
        }

        public IDisposable InfoDuration(string activity)
        {
            return new DurationLogger(s => Info(s), activity);
        }

        public IDisposable InfoDuration(string beginMessage, string endMessage)
        {
            return new DurationLogger(s => Info(s), beginMessage, endMessage);
        }

        [MessageTemplateFormatMethod("format")]
        public void Info(string format, params object[] args)
        {
            _logger.Information(format, args);
        }

        [MessageTemplateFormatMethod("format")]
        public Exception Info(Exception exception, string format, params object[] args)
        {
            _logger.Information(exception, format, args);
            return exception;
        }

        #endregion

        #region debug

        public bool IsDebugEnabled()
        {
            return _logger.IsEnabled(LogEventLevel.Debug);
        }

        public Exception Debug(Exception exception)
        {
            _logger.Warning(exception, string.Empty);
            return exception;
        }

        public IDisposable DebugDuration(string activity)
        {
            return new DurationLogger(s => Debug(s), activity);
        }

        public IDisposable DebugDuration(string beginMessage, string endMessage)
        {
            return new DurationLogger(s => Debug(s), beginMessage, endMessage);
        }

        [MessageTemplateFormatMethod("format")]
        public void Debug(string format, params object[] args)
        {
            _logger.Debug(format, args);
        }

        [MessageTemplateFormatMethod("format")]
        public Exception Debug(Exception exception, string format, params object[] args)
        {
            _logger.Debug(exception, format, args);
            return exception;
        }

        #endregion

        #region trace

        public bool IsTraceEnabled()
        {
            return _logger.IsEnabled(LogEventLevel.Verbose);
        }

        public Exception Trace(Exception exception)
        {
            _logger.Verbose(exception, string.Empty);
            return exception;
        }

        public IDisposable TraceDuration(string activity)
        {
            return new DurationLogger(s => Trace(s), activity);
        }

        public IDisposable TraceDuration(string beginMessage, string endMessage)
        {
            return new DurationLogger(s => Trace(s), beginMessage, endMessage);
        }

        [MessageTemplateFormatMethod("format")]
        public void Trace(string format, params object[] args)
        {
            if (IsTraceEnabled())
            {
                _logger.Verbose(format, args);
            }
        }

        [MessageTemplateFormatMethod("format")]
        public Exception Trace(Exception exception, string format, params object[] args)
        {
            if (IsTraceEnabled())
            {
                _logger.Verbose(exception, format, args);
            }

            return exception;
        }

        #endregion
    }
}
