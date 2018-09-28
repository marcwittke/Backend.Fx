namespace Backend.Fx.Logging
{
    using System;
    using System.Diagnostics;
    using NetFxILogger = Microsoft.Extensions.Logging.ILogger;
    using NetFxLogLevel = Microsoft.Extensions.Logging.LogLevel;

    [DebuggerStepThrough]
    public class FrameworkToBackendFxLogger : NetFxILogger
    {
        private readonly ILogger _logger;
        
        public FrameworkToBackendFxLogger(ILogger logger)
        {
            this._logger = logger;
        }

        public void Log<TState>(NetFxLogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            switch (logLevel)
            {
                case NetFxLogLevel.Critical:
                    _logger.Fatal(exception, formatter(state, exception));
                    return;
                case NetFxLogLevel.Debug:
                    _logger.Debug(exception, formatter(state, exception));
                    return;
                case NetFxLogLevel.Error:
                    _logger.Error(exception, formatter(state, exception));
                    return;
                case NetFxLogLevel.Information:
                    _logger.Info(exception, formatter(state, exception));
                    return;
                case NetFxLogLevel.Trace:
                    _logger.Trace(exception, formatter(state, exception));
                    return;
                case NetFxLogLevel.Warning:
                    _logger.Warn(exception, formatter(state, exception));
                    return;
                default:
                    return;
            }
        }

        public bool IsEnabled(NetFxLogLevel logLevel)
        {
            if (logLevel == NetFxLogLevel.Debug)
            {
                return _logger.IsDebugEnabled();
            }

            if (logLevel == NetFxLogLevel.Trace)
            {
                return _logger.IsTraceEnabled();
            }

            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            LogManager.BeginActivity();
            return _logger.InfoDuration(state.ToString());
        }
    }
}
