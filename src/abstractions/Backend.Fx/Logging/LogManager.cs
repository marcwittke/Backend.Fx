namespace Backend.Fx.Logging
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    [DebuggerStepThrough]
    public abstract class LogManager
    {
        private static int _activityIndex=1;
        private static ILoggerFactory _factory = new DebugLoggerFactory();

        public static void Initialize(ILoggerFactory theFactory)
        {
            _factory = theFactory;
        }

        public static ILogger Create<T>()
        {
            return Create(typeof(T));
        }

        public static ILogger Create(Type t)
        {
            string s = t.FullName;
            var indexOf = s?.IndexOf('[') ?? 0;
            if (indexOf > 0)
            {
                s = s?.Substring(0, indexOf);
            }
            return Create(s);
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