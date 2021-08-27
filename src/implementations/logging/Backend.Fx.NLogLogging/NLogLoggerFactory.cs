using System;
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

        public ILogger Create(Type t)
        {
            string s = t.FullName;
            var indexOf = s?.IndexOf('[') ?? 0;
            if (indexOf > 0)
            {
                s = s?.Substring(0, indexOf);
            }

            return Create(s);
        }

        public ILogger Create<T>()
        {
            return Create(typeof(T));
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