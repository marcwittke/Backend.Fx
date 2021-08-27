using System;
using Backend.Fx.Logging;
using log4net;
using log4net.Core;

namespace Backend.Fx.Log4NetLogging
{
    public class Log4NetLogger : Logging.ILogger
    {
        private readonly ILog _log4NetLogger;

        internal Log4NetLogger(ILog log4NetLogger)
        {
            _log4NetLogger = log4NetLogger;
        }

        public Exception Fatal(Exception exception)
        {
            _log4NetLogger.Fatal(exception);
            return exception;
        }

        public void Fatal(string format, params object[] args)
        {
            _log4NetLogger.FatalFormat(format, args);
        }

        public Exception Fatal(Exception exception, string format, params object[] args)
        {
            _log4NetLogger.Fatal(string.Format(format, args), exception);
            return exception;
        }

        public Exception Error(Exception exception)
        {
            _log4NetLogger.Error(exception);
            return exception;
        }

        public void Error(string format, params object[] args)
        {
            _log4NetLogger.ErrorFormat(format, args);
        }

        public Exception Error(Exception exception, string format, params object[] args)
        {
            _log4NetLogger.Error(string.Format(format, args), exception);
            return exception;
        }

        public Exception Warn(Exception exception)
        {
            _log4NetLogger.Warn(exception);
            return exception;
        }

        public void Warn(string format, params object[] args)
        {
            _log4NetLogger.WarnFormat(format, args);
        }

        public Exception Warn(Exception exception, string format, params object[] args)
        {
            _log4NetLogger.Warn(string.Format(format, args), exception);
            return exception;
        }

        public Exception Info(Exception exception)
        {
            _log4NetLogger.Info(exception);
            return exception;
        }

        public void Info(string message)
        {
            _log4NetLogger.Info(message);
        }

        public IDisposable InfoDuration(string activity)
        {
            return new DurationLogger(Info, activity);
        }

        public IDisposable InfoDuration(string beginMessage, string endMessage)
        {
            return new DurationLogger(Info, beginMessage, endMessage);
        }

        public void Info(string format, params object[] args)
        {
            _log4NetLogger.InfoFormat(format, args);
        }

        public Exception Info(Exception exception, string format, params object[] args)
        {
            _log4NetLogger.Info(string.Format(format, args), exception);
            return exception;
        }

        public bool IsDebugEnabled()
        {
            Level myLevel = Level;
            return myLevel == Level.Debug || myLevel == Level.Fine || myLevel == Level.Trace || myLevel == Level.Finer || myLevel == Level.Verbose || myLevel == Level.Finest;
        }

        public Exception Debug(Exception exception)
        {
            _log4NetLogger.Debug(exception);
            return exception;
        }

        public void Debug(string message)
        {
            _log4NetLogger.Debug(message);
        }

        public IDisposable DebugDuration(string activity)
        {
            return new DurationLogger(Debug, activity);
        }

        public IDisposable DebugDuration(string beginMessage, string endMessage)
        {
            return new DurationLogger(Debug, beginMessage, endMessage);
        }

        public void Debug(string format, params object[] args)
        {
            _log4NetLogger.DebugFormat(format, args);
        }

        public Exception Debug(Exception exception, string format, params object[] args)
        {
            _log4NetLogger.Debug(string.Format(format, args), exception);
            return exception;
        }

        public bool IsTraceEnabled()
        {
            Level myLevel = Level;
            return myLevel == Level.Trace || myLevel == Level.Finer || myLevel == Level.Verbose || myLevel == Level.Finest;
        }

        public Exception Trace(Exception exception)
        {
            _log4NetLogger.Logger.Log(null, Level.Trace, null, exception);
            return exception;
        }

        public void Trace(string message)
        {
            _log4NetLogger.Logger.Log(null, Level.Trace, message, null);
        }

        public IDisposable TraceDuration(string activity)
        {
            return new DurationLogger(Trace, activity);
        }

        public IDisposable TraceDuration(string beginMessage, string endMessage)
        {
            return new DurationLogger(Trace, beginMessage, endMessage);
        }

        public void Trace(string format, params object[] args)
        {
            _log4NetLogger.Logger.Log(null, Level.Trace, string.Format(format, args), null);
        }

        public Exception Trace(Exception exception, string format, params object[] args)
        {
            _log4NetLogger.Logger.Log(null, Level.Trace, string.Format(format, args), exception);
            return exception;
        }

        private Level Level => ((log4net.Repository.Hierarchy.Logger)_log4NetLogger.Logger).EffectiveLevel;
    }
}
