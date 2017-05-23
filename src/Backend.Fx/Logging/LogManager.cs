namespace Backend.Fx.Logging
{
    using System;
    using System.Threading;

    public abstract class LogManager
    {
        private static int activityIndex=1;
        private static ILoggerFactory factory = new DebugLoggerFactory();

        public static void Initialize(ILoggerFactory theFactory)
        {
            factory = theFactory;
        }

        public static ILogger Create<T>()
        {
            return Create(typeof(T));
        }

        public static ILogger Create(Type t)
        {
            return Create(t.FullName);
        }

        public static ILogger Create(string s)
        {
            return factory.Create(s);
        }

        public static void BeginActivity()
        {
            Interlocked.Increment(ref activityIndex);
            factory.BeginActivity(activityIndex);
        }
    }
}