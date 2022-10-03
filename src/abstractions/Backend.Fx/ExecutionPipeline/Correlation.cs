using System;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.ExecutionPipeline
{
    /// <summary>
    /// A guid that is unique for an invocation. In case of an invocation as result of handling an integration event, the correlation
    /// is stable, that is, the correlation can be used to track a logical action over different systems.
    /// </summary>
    [PublicAPI]
    public sealed class Correlation
    {
        private static readonly ILogger Logger = Log.Create<Correlation>();

        public Guid Id { get; private set; } = Guid.NewGuid();

        public void Resume(Guid correlationId)
        {
            Id = correlationId;
            Logger.LogInformation("Resuming correlation {Correlation}", Id);
        }
    }
}