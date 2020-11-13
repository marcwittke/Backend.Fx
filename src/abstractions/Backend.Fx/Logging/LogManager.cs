using System;
using System.Diagnostics;
using System.Threading;

namespace Backend.Fx.Logging
{
    [DebuggerStepThrough]
    public abstract class LogManager
    {
        private static int _activityIndex = 1;
        private static ILoggerFactory _factory = new DebugLoggerFactory();

        public static void Initialize(ILoggerFactory theFactory)
        {
            _factory = theFactory;
        }

        public static ILogger Create<T>()
        {
            return _factory.Create<T>();
        }

        public static ILogger Create(Type t)
        {
            return _factory.Create(t);
        }

        public static ILogger Create(string s)
        {
            return _factory.Create(s);
        }

        public static void BeginActivity()
        {
            Interlocked.Increment(ref _activityIndex);
            _factory.BeginActivity(_activityIndex);
        }

        public static void Shutdown()
        {
            _factory.Shutdown();
        }
    }
}