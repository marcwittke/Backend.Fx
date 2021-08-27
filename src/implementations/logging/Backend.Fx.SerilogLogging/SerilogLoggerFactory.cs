using System;
using System.Diagnostics;
using Backend.Fx.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using ILogger = Backend.Fx.Logging.ILogger;

namespace Backend.Fx.SerilogLogging
{
    [DebuggerStepThrough]
    public class SerilogLoggerFactory : ILoggerFactory
    {
        private readonly Logger _rootLogger;

        public SerilogLoggerFactory(Logger logger = null)
        {
            _rootLogger = logger ?? new LoggerConfiguration().WriteTo.Console(LogEventLevel.Debug).CreateLogger();
        }

        public ILogger Create(string s)
        {
            return TryGetContextTypeFromString(s, out Type t)
                ? new SerilogLogger(_rootLogger.ForContext(t))
                : new SerilogLogger(_rootLogger);
        }

        public ILogger Create(Type t)
        {
            return new SerilogLogger(_rootLogger.ForContext(t));
        }

        public ILogger Create<T>()
        {
            return new SerilogLogger(_rootLogger.ForContext<T>());
        }

        public void BeginActivity(int activityIndex)
        {
            //TODO
        }

        public void Shutdown()
        {
            Log.CloseAndFlush();
        }
        
        private static bool TryGetContextTypeFromString(string s, out Type type)
        {
            try
            {
                type = Type.GetType(s);
            }
            catch
            {
                type = null;
            }
            
            return type != null;
        }
    }
}