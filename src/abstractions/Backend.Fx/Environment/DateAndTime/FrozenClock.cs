﻿using System;
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
        
        public FrozenClock(IClock clock)
        {
            UtcNow = clock.UtcNow;
            Logger.LogTrace("Freezing clock at {UtcNow}", UtcNow);
        }

        public DateTime UtcNow { get; }
    }
}