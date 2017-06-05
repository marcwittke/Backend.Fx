namespace Backend.Fx.NLogLogging
{
    using System.Diagnostics;
    using BackendFxILogger = Backend.Fx.Logging.ILogger;
    using BackendFxILoggerFactory = Backend.Fx.Logging.ILoggerFactory;

    [DebuggerStepThrough]
    public class NLogLoggerFactory : BackendFxILoggerFactory
    {
        public NLogLoggerFactory()
        {
            BeginActivity(0);
        }

        public BackendFxILogger Create(string s)
        {
            return new NLogLogger(NLog.LogManager.GetLogger(s));
        }

        public void BeginActivity(int activityIndex)
        {
            NLog.MappedDiagnosticsLogicalContext.Set("Activity", activityIndex);
        }
    }
}