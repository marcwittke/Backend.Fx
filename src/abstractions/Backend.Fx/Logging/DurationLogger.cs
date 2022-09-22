using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Logging
{
    [PublicAPI]
    [DebuggerStepThrough]
    public class DurationLogger : IDisposable
    {
        private readonly string _endMessage;
        private readonly Action<string> _logAction;

        private readonly Stopwatch _stopwatch = new Stopwatch();

        public DurationLogger(Action<string> logAction, string activity)
            : this(logAction, activity, activity)
        {
        }

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

    public static class DurationLoggerEx
    {
        public static IDisposable LogInformationDuration(this ILogger logger, string activity)
        {
            return new DurationLogger(s => logger.LogInformation(s), activity);
        }

        public static IDisposable LogInformationDuration(this ILogger logger, string beginMessage, string endMessage)
        {
            return new DurationLogger(s => logger.LogInformation(s), beginMessage, endMessage);
        }
        
        public static IDisposable LogDebugDuration(this ILogger logger, string activity)
        {
            return new DurationLogger(s => logger.LogDebug(s), activity);
        }

        public static IDisposable LogDebugDuration(this ILogger logger, string beginMessage, string endMessage)
        {
            return new DurationLogger(s => logger.LogDebug(s), beginMessage, endMessage);
        }
        
        public static IDisposable LogTraceDuration(this ILogger logger, string activity)
        {
            return new DurationLogger(s => logger.LogTrace(s), activity);
        }

        public static IDisposable LogTraceDuration(this ILogger logger, string beginMessage, string endMessage)
        {
            return new DurationLogger(s => logger.LogTrace(s), beginMessage, endMessage);
        }
    }
}