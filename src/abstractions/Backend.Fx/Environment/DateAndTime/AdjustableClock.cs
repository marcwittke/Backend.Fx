using System;
using Backend.Fx.Logging;

namespace Backend.Fx.Environment.DateAndTime
{
    public class AdjustableClock : IClock
    {
        private static readonly ILogger Logger = LogManager.Create<AdjustableClock>();
        
        private readonly IClock _clockImplementation;
        private DateTime? _overriddenUtcNow;

        public AdjustableClock(IClock clockImplementation)
        {
            _clockImplementation = clockImplementation;
        }

        public DateTime UtcNow => _overriddenUtcNow ?? _clockImplementation.UtcNow;

        public void OverrideUtcNow(DateTime utcNow)
        {
            Logger.Trace($"Adjusting clock to {utcNow}");
            _overriddenUtcNow = utcNow;
        }

        public DateTime Advance(TimeSpan timespan)
        {
            _overriddenUtcNow = _overriddenUtcNow ?? _clockImplementation.UtcNow;
            Logger.Trace($"Advancing clock by {timespan}");
            _overriddenUtcNow = _overriddenUtcNow.Value.Add(timespan);
            return _overriddenUtcNow.Value;
        }

        public void ResetToOriginalTime()
        {
            _overriddenUtcNow = null;
        }
    }
}