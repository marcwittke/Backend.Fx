namespace Backend.Fx.Logging
{
    using System.Diagnostics;

    [DebuggerStepThrough]
    public class DebugLoggerFactory : ILoggerFactory
    {
        public ILogger Create(string s)
        {
            return new DebugLogger(s);
        }

        public void BeginActivity(int activityIndex)
        {
            System.Diagnostics.Debug.WriteLine($"Beginning activity {activityIndex}");
        }

        public void Shutdown()
        {}
    }
}
