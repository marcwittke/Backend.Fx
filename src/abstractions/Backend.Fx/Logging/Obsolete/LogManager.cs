using System;
using System.Threading;
using Microsoft.Extensions.Logging.Abstractions;

namespace Backend.Fx.Logging
{
    [Obsolete]
    public static class LogManager
    {
        private static ILoggerFactory _loggerFactory = new BackendFxToMicrosoftLoggingLoggerFactory(new NullLoggerFactory());
        private static int _activityIndex;

        public static void Initialize(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public static ILogger Create<T>()
        {
            return _loggerFactory.Create(typeof(T).FullName);
        }

        public static ILogger Create(Type t)
        {
            return _loggerFactory.Create(t.FullName);
        }

        public static ILogger Create(string category)
        {
            return _loggerFactory.Create(category);
        }

        public static void BeginActivity()
        {
            Interlocked.Increment(ref _activityIndex);
            _loggerFactory.BeginActivity(_activityIndex);
        }

        public static void Shutdown()
        {
            _loggerFactory.Shutdown();
        }
    }
}
