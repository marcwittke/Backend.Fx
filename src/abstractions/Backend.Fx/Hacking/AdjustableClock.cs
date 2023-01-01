using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Backend.Fx.Hacking
{
    [PublicAPI]
    public class AdjustableClock : IClock
    {
        private readonly ILogger _logger = Log.Create<AdjustableClock>();
        
        private readonly IClock _clockImplementation;
        private Instant? _overriddenUtcNow;

        public AdjustableClock(IClock clockImplementation)
        {
            _clockImplementation = clockImplementation;
        }

        public Instant GetCurrentInstant() => _overriddenUtcNow ?? _clockImplementation.GetCurrentInstant();

        public void OverrideUtcNow(Instant instant)
        {
            _logger.LogTrace("Adjusting clock to {Instant}", instant);
            _overriddenUtcNow = instant;
        }

        public Instant Advance(Duration duration)
        {
            _overriddenUtcNow ??= _clockImplementation.GetCurrentInstant();
            _logger.LogTrace("Advancing clock by {TimeSpan}", duration);
            _overriddenUtcNow = _overriddenUtcNow.Value.Plus(duration);
            return _overriddenUtcNow.Value;
        }
    }
}