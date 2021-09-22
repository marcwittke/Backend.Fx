using System;
using System.Diagnostics;
using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.NetCore.Logging
{
    using NetFxILogger = ILogger;
    using NetFxLogLevel = LogLevel;


    [DebuggerStepThrough]
    public class FrameworkToBackendFxLogger : NetFxILogger
    {
        private readonly Fx.Logging.ILogger _logger;

        public FrameworkToBackendFxLogger(Fx.Logging.ILogger logger)
        {
            _logger = logger;
        }

        public void Log<TState>(
            NetFxLogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            string formatted = formatter(state, exception);
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
