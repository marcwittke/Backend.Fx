using System;
using Backend.Fx.Logging;

namespace Backend.Fx.Patterns.DependencyInjection
{
    public class Correlation
    {
        private static readonly ILogger Logger = LogManager.Create<Correlation>();

        public Guid Id { get; private set; } = Guid.NewGuid();

        public void Resume(Guid correlationId)
        {
            Logger.Info($"Resuming correlation {correlationId}");
            Id = correlationId;
        }
    }
}