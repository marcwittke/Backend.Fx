using System;
using System.Diagnostics;

namespace Backend.Fx.Logging
{
    [DebuggerStepThrough]
    public class DurationLogger : IDisposable
    {
        private readonly string _endMessage;
        private readonly Action<string> _logAction;

        private readonly Stopwatch _stopwatch = new Stopwatch();

        public DurationLogger(Action<string> logAction, string activity)
            : this(logAction, activity, activity)
        { }

        public DurationLogger(Action<string> logAction, string beginMessage, string endMessage)
        {
            _logAction = logAction;
            _endMessage = endMessage;
            _logAction(beginMessage);
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
                return $"{activity} - Duration: {duration.TotalMilliseconds:0}ms";
            }

            if (duration.TotalSeconds < 60)
            {
                return $"{activity} - Duration: {duration.TotalSeconds:0.00}s";
            }

            return $"{activity} - Duration: {duration:g}";
        }
    }
}
