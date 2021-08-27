using System;
using System.Diagnostics;

namespace Backend.Fx.Logging
{
    using NetFxILogger = Microsoft.Extensions.Logging.ILogger;
    using NetFxLogLevel = Microsoft.Extensions.Logging.LogLevel;

    [DebuggerStepThrough]
    public class FrameworkToBackendFxLogger : NetFxILogger
    {
        private readonly ILogger _logger;

        public FrameworkToBackendFxLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void Log<TState>(NetFxLogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var formatted = formatter(state, exception);
            formatted = formatted?.Replace("{", "{{").Replace("}", "}}");

            switch (logLevel)
            {
                case NetFxLogLevel.Critical:
                    _logger.Fatal(exception, formatted);
                    return;
                case NetFxLogLevel.Debug:
                    _logger.Debug(exception, formatted);
                    return;
                case NetFxLogLevel.Error:
                    _logger.Error(exception, formatted);
                    return;
                case NetFxLogLevel.Information:
                    _logger.Info(exception, formatted);
                    return;
                case NetFxLogLevel.Trace:
                    _logger.Trace(exception, formatted);
                    return;
                case NetFxLogLevel.Warning:
                    _logger.Warn(exception, formatted);
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