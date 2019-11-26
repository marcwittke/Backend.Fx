using System.Diagnostics;

namespace Backend.Fx.Logging
{
    [DebuggerStepThrough]
    public class DebugLoggerFactory : ILoggerFactory
    {
        public ILogger Create(string s)
        {
            return new DebugLogger(s);
        }

        public void BeginActivity(int activityIndex)
        {
            Debug.WriteLine($"Beginning activity {activityIndex}");
        }

        public void Shutdown()
        {}
    }
}
