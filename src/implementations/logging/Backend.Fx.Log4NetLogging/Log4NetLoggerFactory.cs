using System;
using System.Diagnostics;
using System.Reflection;
using log4net;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using BackendFxILogger = Backend.Fx.Logging.ILogger;
using BackendFxILoggerFactory = Backend.Fx.Logging.ILoggerFactory;

namespace Backend.Fx.Log4NetLogging
{
    [DebuggerStepThrough]
    public class Log4NetLoggerFactory : BackendFxILoggerFactory
    {
        private readonly ILoggerRepository _loggerRepository;

        public Log4NetLoggerFactory()
        {
            _loggerRepository = LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(Hierarchy));

            BeginActivity(0);
        }

        public BackendFxILogger Create(string s)
        {
            return new Log4NetLogger(LogManager.GetLogger(_loggerRepository.Name, s));
        }

        public BackendFxILogger Create(Type t)
        {
            string s = t.FullName;
            int indexOf = s?.IndexOf('[') ?? 0;
            if (indexOf > 0)
            {
                s = s?.Substring(0, indexOf);
            }

            return Create(s);
        }

        public BackendFxILogger Create<T>()
        {
            return Create(typeof(T));
        }

        public void BeginActivity(int activityIndex)
        {
            LogicalThreadContext.Properties["app-Activity"] = activityIndex;
        }

        public void Shutdown()
        {
            LogManager.Flush(10000);
        }
    }
}
