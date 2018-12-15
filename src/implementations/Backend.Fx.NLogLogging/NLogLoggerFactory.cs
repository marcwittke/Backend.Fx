namespace Backend.Fx.NLogLogging
{
    using System.Diagnostics;
    
    [DebuggerStepThrough]
    public class NLogLoggerFactory : Backend.Fx.Logging.ILoggerFactory
    {
        public NLogLoggerFactory()
        {
            BeginActivity(0);
        }

        public Backend.Fx.Logging.ILogger Create(string s)
        {
            return new NLogLogger(NLog.LogManager.GetLogger(s));
        }

        public void BeginActivity(int activityIndex)
        {
            NLog.MappedDiagnosticsLogicalContext.Set("app-Activity", activityIndex);
        }

        public void Shutdown()
        {
            NLog.LogManager.Shutdown();
        }

        public static void Configure(string nlogConfigPath)
        {
            Backend.Fx.Logging.LogManager.Initialize(new NLogLoggerFactory());
            NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(nlogConfigPath);
        }
    }
}