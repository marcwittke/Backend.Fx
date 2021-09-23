using System;
using Backend.Fx.Logging;

namespace Backend.Fx.Environment.DateAndTime
{
    /// <summary>
    /// Best practice for web (service) applications: time does not advance during a single request
    /// </summary>
    public class FrozenClock : IClock
    {
        private static readonly ILogger Logger = LogManager.Create<FrozenClock>();

        // ReSharper disable once UnusedParameter.Local
        public FrozenClock(IClock clock)
        {
            UtcNow = clock.UtcNow;
            Logger.Trace($"Freezing clock at {UtcNow}");
        }

        public DateTime UtcNow { get; }
    }
}
