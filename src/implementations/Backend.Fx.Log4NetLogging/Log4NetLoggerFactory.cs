using System.Diagnostics;
using System.Reflection;

namespace Backend.Fx.Log4NetLogging
{
    using log4net.Repository;
    using BackendFxILogger = Backend.Fx.Logging.ILogger;
    using BackendFxILoggerFactory = Backend.Fx.Logging.ILoggerFactory;

    [DebuggerStepThrough]
    public class Log4NetLoggerFactory : BackendFxILoggerFactory
    {
        private readonly ILoggerRepository _loggerRepository;

        public Log4NetLoggerFactory()
        {
            _loggerRepository = log4net.LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
            
            BeginActivity(0);
        }

        public BackendFxILogger Create(string s)
        {
            return new Log4NetLogger(log4net.LogManager.GetLogger(_loggerRepository.Name, s));
        }

        public void BeginActivity(int activityIndex)
        {
            log4net.LogicalThreadContext.Properties["app-Activity"] = activityIndex;
        }

        public void Shutdown()
        {
            log4net.LogManager.Flush(10000);
        }
    }
}
