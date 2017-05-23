namespace Backend.Fx.NLogLogging
{
    using System.Diagnostics;
    using NetFxILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
    using NetFxILoggerProvider = Microsoft.Extensions.Logging.ILoggerProvider;
    using NLogLogManager = NLog.LogManager;

    [DebuggerStepThrough]
    public class FrameworkToNLogLoggerFactory : NetFxILoggerFactory
    {
        public void Dispose() { }

        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            return new FrameworkToNLogLogger(NLogLogManager.GetLogger(categoryName));
        }

        public void AddProvider(NetFxILoggerProvider provider)
        { }
    }
}
