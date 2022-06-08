using System;
using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Environment.DateAndTime
{
    /// <summary>
    /// Best practice for web (service) applications: time does not advance during a single request
    /// </summary>
    public class FrozenClock : IClock
    {
        private static readonly ILogger Logger = Log.Create<FrozenClock>();
        
        // ReSharper disable once UnusedParameter.Local
        public FrozenClock(IClock clock)
        {
            UtcNow = DateTime.UtcNow;
            Logger.LogTrace("Freezing clock at {UtcNow} UTC", UtcNow);
        }

        public DateTime UtcNow { get; }
    }
}