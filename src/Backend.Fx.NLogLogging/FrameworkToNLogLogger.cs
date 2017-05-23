namespace Backend.Fx.NLogLogging
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using NetFxILogger = Microsoft.Extensions.Logging.ILogger;
    using NetFxLogLevel = Microsoft.Extensions.Logging.LogLevel;

    using NLogILogger = NLog.ILogger;
    using NLogLogLevel = NLog.LogLevel;

    [DebuggerStepThrough]
    public class FrameworkToNLogLogger : NetFxILogger
    {
        private readonly NLogILogger logger;

        public FrameworkToNLogLogger(NLogILogger logger)
        {
            this.logger = logger;
        }

        public void Log<TState>(NetFxLogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            logger.Log(Map(logLevel), exception, CultureInfo.InvariantCulture, formatter(state, exception));
        }

        private NLogLogLevel Map(NetFxLogLevel logLevel)
        {
            switch(logLevel) 
            {
                case NetFxLogLevel.Critical: return NLogLogLevel.Fatal;
                case NetFxLogLevel.Debug: return NLogLogLevel.Debug;
                case NetFxLogLevel.Error: return NLogLogLevel.Error;
                case NetFxLogLevel.Information: return NLogLogLevel.Info;
                case NetFxLogLevel.Trace: return NLogLogLevel.Trace;
                case NetFxLogLevel.Warning: return NLogLogLevel.Warn;
                //case Microsoft.Extensions.Logging.LogLevel.None: 
                default:
                    return NLogLogLevel.Debug;
            }
        }

        public bool IsEnabled(NetFxLogLevel logLevel)
        {
            return logger.IsEnabled(Map(logLevel));
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}
