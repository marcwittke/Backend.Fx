using System;
using System.Diagnostics;
using System.Globalization;
using Backend.Fx.Logging;
using NLog;

namespace Backend.Fx.NLogLogging
{
    using BackendFxILogger = Logging.ILogger;
    using NLogILogger = NLog.ILogger;
    using NLogLogLevel = LogLevel;

    [DebuggerStepThrough]
    public class NLogLogger : BackendFxILogger
    {
        private readonly NLogILogger _nlogLogger;

        internal NLogLogger(NLogILogger nlogLogger)
        {
            _nlogLogger = nlogLogger;
        }

        #region fatal

        public Exception Fatal(Exception exception)
        {
            _nlogLogger.Fatal(exception);
            return exception;
        }

        public void Fatal(string format, params object[] args)
        {
            _nlogLogger.Fatal(CultureInfo.InvariantCulture, format, args);
        }

        public Exception Fatal(Exception exception, string format, params object[] args)
        {
            _nlogLogger.Fatal(exception, CultureInfo.InvariantCulture, format, args);
            return exception;
        }

        #endregion

        #region error

        public Exception Error(Exception exception)
        {
            _nlogLogger.Error(exception);
            return exception;
        }

        public void Error(string format, params object[] args)
        {
            _nlogLogger.Error(CultureInfo.InvariantCulture, format, args);
        }

        public Exception Error(Exception exception, string format, params object[] args)
        {
            _nlogLogger.Error(exception, CultureInfo.InvariantCulture, format, args);
            return exception;
        }

        #endregion

        #region warn

        public Exception Warn(Exception exception)
        {
            _nlogLogger.Warn(exception);
            return exception;
        }

        public void Warn(string format, params object[] args)
        {
            _nlogLogger.Warn(CultureInfo.InvariantCulture, format, args);
        }

        public Exception Warn(Exception exception, string format, params object[] args)
        {
            _nlogLogger.Warn(exception, CultureInfo.InvariantCulture, format, args);
            return exception;
        }

        #endregion

        #region info

        public Exception Info(Exception exception)
        {
            _nlogLogger.Info(exception);
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

        public void Info(string format, params object[] args)
        {
            _nlogLogger.Info(CultureInfo.InvariantCulture, format, args);
        }

        public Exception Info(Exception exception, string format, params object[] args)
        {
            _nlogLogger.Info(exception, CultureInfo.InvariantCulture, format, args);
            return exception;
        }

        #endregion

        #region debug

        public bool IsDebugEnabled()
        {
            return _nlogLogger.IsDebugEnabled;
        }

        public Exception Debug(Exception exception)
        {
            _nlogLogger.Warn(exception);
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

        public void Debug(string format, params object[] args)
        {
            _nlogLogger.Debug(CultureInfo.InvariantCulture, format, args);
        }

        public Exception Debug(Exception exception, string format, params object[] args)
        {
            _nlogLogger.Debug(exception, CultureInfo.InvariantCulture, format, args);
            return exception;
        }

        #endregion

        #region trace

        public bool IsTraceEnabled()
        {
            return _nlogLogger.IsEnabled(NLogLogLevel.Trace);
        }

        public Exception Trace(Exception exception)
        {
            _nlogLogger.Trace(exception);
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

        public void Trace(string format, params object[] args)
        {
            if (IsTraceEnabled()) _nlogLogger.Trace(CultureInfo.InvariantCulture, format, args);
        }

        public Exception Trace(Exception exception, string format, params object[] args)
        {
            if (IsTraceEnabled()) _nlogLogger.Trace(exception, CultureInfo.InvariantCulture, format, args);

            return exception;
        }

        #endregion
    }
}