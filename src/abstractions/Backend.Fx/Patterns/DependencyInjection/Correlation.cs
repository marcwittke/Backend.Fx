using System;
using Backend.Fx.Logging;

namespace Backend.Fx.Patterns.DependencyInjection
{
    /// <summary>
    /// A guid that is unique for an invocation. In case of an invocation as result of handling an integration event, the
    /// correlation
    /// is stable, that is, the correlation can be used to track a logical action over different systems.
    /// </summary>
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
