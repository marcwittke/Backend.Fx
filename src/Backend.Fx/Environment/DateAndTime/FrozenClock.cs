namespace Backend.Fx.Environment.DateAndTime
{
    using System;

    /// <summary>
    /// Best practice for web (service) applications: time does not advance during a single request
    /// </summary>
    public class FrozenClock : Clock
    {
        public FrozenClock()
        {
            OverrideUtcNow(DateTime.UtcNow);
        }
    }
}