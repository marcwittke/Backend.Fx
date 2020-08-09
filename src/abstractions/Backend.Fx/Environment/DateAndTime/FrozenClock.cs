using System;

namespace Backend.Fx.Environment.DateAndTime
{
    /// <summary>
    /// Best practice for web (service) applications: time does not advance during a single request
    /// </summary>
    public class FrozenClock : IClock
    {
        // ReSharper disable once UnusedParameter.Local
        public FrozenClock(IClock clock)
        {
            UtcNow = DateTime.UtcNow;
        }

        public DateTime UtcNow { get; }
    }
}