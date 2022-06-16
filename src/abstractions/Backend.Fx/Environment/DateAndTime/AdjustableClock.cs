using System;
using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Environment.DateAndTime
{
    public class AdjustableClock : IClock
    {
        private static readonly ILogger Logger = Log.Create<AdjustableClock>();
        
        private readonly IClock _clockImplementation;
        private DateTime? _overriddenUtcNow;

        public AdjustableClock(IClock clockImplementation)
        {
            _clockImplementation = clockImplementation;
        }

        public DateTime UtcNow => _overriddenUtcNow ?? _clockImplementation.UtcNow;

        public void OverrideUtcNow(DateTime utcNow)
        {
            Logger.LogTrace("Adjusting clock to {UtcNow}", utcNow);
            if (utcNow.Kind != DateTimeKind.Utc)
            {
                utcNow = new DateTime(utcNow.Ticks, DateTimeKind.Utc);
            }
            
            _overriddenUtcNow = utcNow;
        }

        public DateTime Advance(TimeSpan timespan)
        {
            _overriddenUtcNow = _overriddenUtcNow ?? _clockImplementation.UtcNow;
            Logger.LogTrace("Advancing clock by {TimeSpan}", timespan);
            _overriddenUtcNow = _overriddenUtcNow.Value.Add(timespan);
            return _overriddenUtcNow.Value;
        }
    }
}