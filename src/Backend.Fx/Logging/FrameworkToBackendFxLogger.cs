namespace Backend.Fx.Logging
{
    using System;
    using System.Diagnostics;
    using NetFxILogger = Microsoft.Extensions.Logging.ILogger;
    using NetFxLogLevel = Microsoft.Extensions.Logging.LogLevel;

    [DebuggerStepThrough]
    public class FrameworkToBackendFxLogger : NetFxILogger
    {
        private readonly ILogger logger;
        
        public FrameworkToBackendFxLogger(ILogger logger)
        {
            this.logger = logger;
        }

        public void Log<TState>(NetFxLogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            switch (logLevel)
            {
                case NetFxLogLevel.Critical:
                    logger.Fatal(exception, formatter(state, exception));
                    return;
                case NetFxLogLevel.Debug:
                    logger.Debug(exception, formatter(state, exception));
                    return;
                case NetFxLogLevel.Error:
                    logger.Error(exception, formatter(state, exception));
                    return;
                case NetFxLogLevel.Information:
                    logger.Info(exception, formatter(state, exception));
                    return;
                case NetFxLogLevel.Trace:
                    logger.Trace(exception, formatter(state, exception));
                    return;
                case NetFxLogLevel.Warning:
                    logger.Warn(exception, formatter(state, exception));
                    return;
                default:
                    return;
            }
        }

        public bool IsEnabled(NetFxLogLevel logLevel)
        {
            if (logLevel == NetFxLogLevel.Debug)
            {
                return logger.IsDebugEnabled();
            }

            if (logLevel == NetFxLogLevel.Trace)
            {
                return logger.IsTraceEnabled();
            }

            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            LogManager.BeginActivity();
            return logger.InfoDuration(state.ToString());
        }
    }
}
