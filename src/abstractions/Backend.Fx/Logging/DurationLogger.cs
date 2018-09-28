namespace Backend.Fx.Logging
{
    using System;
    using System.Diagnostics;

    [DebuggerStepThrough]
    public class DurationLogger : IDisposable
    {
        private readonly string _endMessage;
        private readonly Action<string> _logAction;

        private readonly Stopwatch _stopwatch = new Stopwatch();

        public DurationLogger(Action<string> logAction, string activity)
            : this(logAction, activity, activity) {}

        public DurationLogger(Action<string> logAction, string beginMessage, string endMessage)
        {
            this._logAction = logAction;
            this._endMessage = endMessage;
            this._logAction(beginMessage);
            _stopwatch.Start();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _logAction.Invoke(FormatDuration(_endMessage, _stopwatch.Elapsed));
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