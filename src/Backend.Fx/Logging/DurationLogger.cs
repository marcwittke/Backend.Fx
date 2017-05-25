namespace Backend.Fx.Logging
{
    using System;
    using System.Diagnostics;

    public class DurationLogger : IDisposable
    {
        private readonly string endMessage;
        private readonly Action<string> logAction;

        private readonly Stopwatch stopwatch = new Stopwatch();

        public DurationLogger(Action<string> logAction, string activity)
            : this(logAction, activity, activity) {}

        public DurationLogger(Action<string> logAction, string beginMessage, string endMessage)
        {
            this.logAction = logAction;
            this.endMessage = endMessage;
            this.logAction(beginMessage);
            stopwatch.Start();
        }

        public void Dispose()
        {
            stopwatch.Stop();
            logAction.Invoke(FormatDuration(endMessage, stopwatch.Elapsed));
        }

        private static string FormatDuration(string activity, TimeSpan duration)
        {
            if (duration.TotalMilliseconds < 1000)
            {
                return string.Format("{0} - Duration: {1:0}ms", activity, duration.TotalMilliseconds);
            }

            if (duration.TotalSeconds < 60)
            {
                return string.Format("{0} - Duration: {1:0.00}s", activity, duration.TotalSeconds);
            }

            return string.Format("{0} - Duration: {1:g}", activity, duration);
        }
    }
}