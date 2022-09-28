using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Backend.Fx.ExecutionPipeline
{
    /// <summary>
    /// Best practice for web (service) applications: time does not advance during an invocation
    /// </summary>
    public class FrozenClock : IClock
    {
        private static readonly ILogger Logger = Log.Create<FrozenClock>();
        private readonly Instant _frozenInstant;

        public FrozenClock(IClock clock)
        {
            _frozenInstant = clock.GetCurrentInstant();
            Logger.LogTrace("Freezing clock at {Instant}", _frozenInstant);
        }


        public Instant GetCurrentInstant()
        {
            return _frozenInstant;
        }
    }
}