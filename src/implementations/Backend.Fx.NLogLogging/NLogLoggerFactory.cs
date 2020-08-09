using System.Diagnostics;
using Backend.Fx.Logging;
using NLog;
using NLog.Config;
using ILogger = Backend.Fx.Logging.ILogger;
using LogManager = NLog.LogManager;

namespace Backend.Fx.NLogLogging
{
    [DebuggerStepThrough]
    public class NLogLoggerFactory : ILoggerFactory
    {
        public NLogLoggerFactory()
        {
            BeginActivity(0);
        }

        public ILogger Create(string s)
        {
            return new NLogLogger(LogManager.GetLogger(s));
        }

        public void BeginActivity(int activityIndex)
        {
            MappedDiagnosticsLogicalContext.Set("app-Activity", activityIndex);
        }

        public void Shutdown()
        {
            LogManager.Shutdown();
        }

        public static void Configure(string nlogConfigPath)
        {
            Logging.LogManager.Initialize(new NLogLoggerFactory());
            LogManager.Configuration = new XmlLoggingConfiguration(nlogConfigPath);
        }
    }
}