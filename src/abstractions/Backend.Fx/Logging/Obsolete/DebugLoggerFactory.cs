using System;
using System.Diagnostics;

// ReSharper disable CheckNamespace
namespace Backend.Fx.Logging
{
    [DebuggerStepThrough]
    [Obsolete]
    public class DebugLoggerFactory : ILoggerFactory
    {
        public ILogger Create(string s)
        {
            return new DebugLogger(s);
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
            Debug.WriteLine($"Beginning activity {activityIndex}");
        }

        public void Shutdown()
        {
        }
    }
}