using System;

namespace Backend.Fx.Environment.DateAndTime
{
    /// <summary>
    /// The real system clock
    /// </summary>
    public class WallClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}