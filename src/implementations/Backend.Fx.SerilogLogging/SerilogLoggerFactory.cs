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
            throw new NotSupportedException();
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
            _rootLogger.Dispose();
        }
    }
}