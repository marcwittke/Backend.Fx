using System;
using System.Diagnostics;
using Serilog;
using Serilog.Events;

namespace Backend.Fx.SerilogLogging
{
    [DebuggerStepThrough]
    public class SerilogLogger : Logging.ILogger
    {
        private readonly ILogger _logger;

        internal SerilogLogger(ILogger logger)
        {
            _logger = logger;
        }
        
        #region fatal

        public Exception Fatal(Exception exception)
        {
            _logger.Fatal(exception, string.Empty);
            return exception;
        }

        public void Fatal(string format, params object[] args)
        {
            _logger.Fatal(format, args);
        }

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

        public void Error(string format, params object[] args)
        {
            _logger.Error(format, args);
        }

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

        public void Warn(string format, params object[] args)
        {
            _logger.Warning(format, args);
        }

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
            return new Logging.DurationLogger(s => Info(s), activity);
        }

        public IDisposable InfoDuration(string beginMessage, string endMessage)
        {
            return new Logging.DurationLogger(s => Info(s), beginMessage, endMessage);
        }

        public void Info(string format, params object[] args)
        {
            _logger.Information(format, args);
        }

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
            return new Logging.DurationLogger(s => Debug(s), activity);
        }

        public IDisposable DebugDuration(string beginMessage, string endMessage)
        {
            return new Logging.DurationLogger(s => Debug(s), beginMessage, endMessage);
        }

        public void Debug(string format, params object[] args)
        {
            _logger.Debug(format, args);
        }

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
            return new Logging.DurationLogger(s => Trace(s), activity);
        }

        public IDisposable TraceDuration(string beginMessage, string endMessage)
        {
            return new Logging.DurationLogger(s => Trace(s), beginMessage, endMessage);
        }

        public void Trace(string format, params object[] args)
        {
            if (IsTraceEnabled()) _logger.Verbose(format, args);
        }

        public Exception Trace(Exception exception, string format, params object[] args)
        {
            if (IsTraceEnabled()) _logger.Verbose(exception, format, args);

            return exception;
        }

        #endregion
    }
}