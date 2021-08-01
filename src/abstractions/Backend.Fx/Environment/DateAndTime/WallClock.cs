using System;
using Backend.Fx.BuildingBlocks;

namespace Backend.Fx.Environment.DateAndTime
{
    /// <summary>
    /// The real system clock
    /// </summary>
    public class WallClock : IClock, IApplicationService
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}