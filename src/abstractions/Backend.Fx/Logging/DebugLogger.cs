namespace Backend.Fx.Logging
{
    using System;
    using System.Diagnostics;

    [DebuggerStepThrough]
    public class DebugLogger : ILogger
    {
        private readonly string _type;

        public DebugLogger(string type)
        {
            _type = type;
        }

        public Exception Fatal(Exception exception)
        {
            PrintToDebug(exception);
            return exception;
        }

        public void Fatal(string format, params object[] args)
        {
            PrintToDebug(format, args);
        }

        public void Error(string format, params object[] args)
        {
            PrintToDebug(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            PrintToDebug(format, args);
        }

        public IDisposable InfoDuration(string activity)
        {
            return new DurationLogger(s => System.Diagnostics.Debug.WriteLine(s), activity);
        }

        public IDisposable InfoDuration(string beginMessage, string endMessage)
        {
            return new DurationLogger(s => System.Diagnostics.Debug.WriteLine(s), beginMessage, endMessage);
        }

        public void Info(string format, params object[] args)
        {
            PrintToDebug(format, args);
        }

        public bool IsDebugEnabled()
        {
            return true;
        }

        public IDisposable DebugDuration(string activity)
        {
            return new DurationLogger(s => System.Diagnostics.Debug.WriteLine(s), activity);
        }

        public IDisposable DebugDuration(string beginMessage, string endMessage)
        {
            return new DurationLogger(s => System.Diagnostics.Debug.WriteLine(s), beginMessage, endMessage);
        }

        public void Debug(string format, params object[] args)
        {
            PrintToDebug(format, args);
        }

        public bool IsTraceEnabled()
        {
            return true;
        }

        public IDisposable TraceDuration(string activity)
        {
            return new DurationLogger(s => System.Diagnostics.Debug.WriteLine(s), activity);
        }

        public IDisposable TraceDuration(string beginMessage, string endMessage)
        {
            return new DurationLogger(s => System.Diagnostics.Debug.WriteLine(s), beginMessage, endMessage);
        }

        public void Trace(string format, params object[] args)
        {
            PrintToDebug(format, args);
        }

        public Exception Trace(Exception exception, string format, params object[] args)
        {
            PrintToDebug(exception);
            PrintToDebug(format, args);
            return exception;
        }

        public Exception Trace(Exception exception)
        {
            PrintToDebug(exception);
            return exception;
        }

        public Exception Debug(Exception exception, string format, params object[] args)
        {
            PrintToDebug(format, args);
            return exception;
        }

        public Exception Debug(Exception exception)
        {
            PrintToDebug(exception);
            return exception;
        }

        public Exception Info(Exception exception, string format, params object[] args)
        {
            PrintToDebug(exception);
            PrintToDebug(format, args);
            return exception;
        }

        public Exception Info(Exception exception)
        {
            PrintToDebug(exception);
            return exception;
        }

        public Exception Warn(Exception exception, string format, params object[] args)
        {
            PrintToDebug(exception);
            PrintToDebug(format, args);
            return exception;
        }

        public Exception Warn(Exception exception)
        {
            PrintToDebug(exception);
            return exception;
        }

        public Exception Error(Exception exception, string format, params object[] args)
        {
            PrintToDebug(exception);
            PrintToDebug(format, args);
            return exception;
        }

        public Exception Error(Exception exception)
        {
            PrintToDebug(exception);
            return exception;
        }

        public Exception Fatal(Exception exception, string format, params object[] args)
        {
            PrintToDebug(exception);
            PrintToDebug(format, args);
            return exception;
        }

        private void PrintToDebug(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"{_type} {ex}");
        }

        private void PrintToDebug(string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(_type + format, args);
        }

    }
}