// ReSharper disable UnusedMethodReturnValue.Global
using System;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace
namespace Backend.Fx.Logging
{
    [Obsolete]
    public interface ILogger
    {
        #region fatal

        Exception Fatal(Exception exception);

        [StringFormatMethod("format")]
        void Fatal(string format, params object[] args);

        [StringFormatMethod("format")]
        Exception Fatal(Exception exception, string format, params object[] args);

        #endregion

        #region error

        Exception Error(Exception exception);

        [StringFormatMethod("format")]
        void Error(string format, params object[] args);

        [StringFormatMethod("format")]
        Exception Error(Exception exception, string format, params object[] args);

        #endregion

        #region warn

        Exception Warn(Exception exception);

        [StringFormatMethod("format")]
        void Warn(string format, params object[] args);

        [StringFormatMethod("format")]
        Exception Warn(Exception exception, string format, params object[] args);

        #endregion

        #region info

        Exception Info(Exception exception);

        IDisposable InfoDuration(string activity);

        IDisposable InfoDuration(string beginMessage, string endMessage);

        [StringFormatMethod("format")]
        void Info(string format, params object[] args);

        [StringFormatMethod("format")]
        Exception Info(Exception exception, string format, params object[] args);

        #endregion

        #region debug

        bool IsDebugEnabled();

        Exception Debug(Exception exception);

        IDisposable DebugDuration(string activity);

        IDisposable DebugDuration(string beginMessage, string endMessage);

        [StringFormatMethod("format")]
        void Debug(string format, params object[] args);

        [StringFormatMethod("format")]
        Exception Debug(Exception exception, string format, params object[] args);

        #endregion

        #region Trace

        bool IsTraceEnabled();

        Exception Trace(Exception exception);

        IDisposable TraceDuration(string activity);

        IDisposable TraceDuration(string beginMessage, string endMessage);

        [StringFormatMethod("format")]
        void Trace(string format, params object[] args);

        [StringFormatMethod("format")]
        Exception Trace(Exception exception, string format, params object[] args);

        #endregion
    }
}