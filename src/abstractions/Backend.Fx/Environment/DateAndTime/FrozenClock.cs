using System;

namespace Backend.Fx.Environment.DateAndTime
{
    /// <summary>
    /// Best practice for web (service) applications: time does not advance during a single request
    /// </summary>
    public class FrozenClock : Clock
    {
        public FrozenClock() : this(DateTime.UtcNow)
        {
        }

        private FrozenClock(DateTime utcNow)
        {
            OverrideUtcNow(utcNow);
        }

        public static IClock WithFrozenUtcNow(DateTime utcNow)
        {
            return new FrozenClock(utcNow);
        }
    }
}