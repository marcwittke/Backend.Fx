namespace Backend.Fx.Environment.DateAndTime
{
    using System;
    using Logging;

    public abstract class Clock : IClock
    {
        private static readonly ILogger Logger = LogManager.Create<Clock>();

        private DateTime? utcNow;

        public DateTime UtcNow
        {
            get { return utcNow ?? DateTime.UtcNow; }
        }

        public DateTime GetLocalNow(int utcOffsetMinutes)
        {
            return UtcNow.AddMinutes(utcOffsetMinutes);
        }

        public DateTime LocalizeUtcDateTime(DateTime utcDateTime, int utcOffsetMinutes)
        {
            return utcDateTime.AddMinutes(utcOffsetMinutes);
        }

        public void OverrideUtcNow(DateTime overriddenUtcNow)
        {
            Logger.Debug("Freezing clock at {0:O}", overriddenUtcNow);
            utcNow = overriddenUtcNow;
        }

        public DateTime? ToUtcTime(DateTime? localDateTime, int utcOffsetMinutes)
        {
            return localDateTime?.AddMinutes(-utcOffsetMinutes);
        }
    }
}