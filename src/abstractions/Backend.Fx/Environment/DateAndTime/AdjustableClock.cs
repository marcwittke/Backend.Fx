using System;

namespace Backend.Fx.Environment.DateAndTime
{
    public class AdjustableClock : IClock
    {
        private readonly IClock _clockImplementation;
        private DateTime? _overriddenUtcNow;

        public AdjustableClock(IClock clockImplementation)
        {
            _clockImplementation = clockImplementation;
        }

        public DateTime UtcNow => _overriddenUtcNow ?? _clockImplementation.UtcNow;

        public void OverrideUtcNow(DateTime utcNow)
        {
            _overriddenUtcNow = utcNow;
        }

        public DateTime Advance(TimeSpan timespan)
        {
            _overriddenUtcNow = _overriddenUtcNow ?? _clockImplementation.UtcNow;
            _overriddenUtcNow = _overriddenUtcNow.Value.Add(timespan);
            return _overriddenUtcNow.Value;
        }
    }
}