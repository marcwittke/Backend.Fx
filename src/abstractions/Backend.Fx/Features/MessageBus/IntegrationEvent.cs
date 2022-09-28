using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NodaTime;

namespace Backend.Fx.Features.MessageBus
{
    [PublicAPI]
    public interface IIntegrationEvent
    {
        Guid Id { get; }
        Instant CreationDate { get; }
        Guid CorrelationId { get; }
        
        Dictionary<string, string> Properties { get; }
    }

    /// <summary>
    /// Events that should be handled in a separate context. Might be persisted as well using an external message bus.
    /// See https://blogs.msdn.microsoft.com/cesardelatorre/2017/02/07/domain-events-vs-integration-events-in-domain-driven-design-and-microservices-architectures/
    /// </summary>
    public abstract class IntegrationEvent : IIntegrationEvent
    {
        public Guid Id { get; } = Guid.NewGuid();

        
        public Instant CreationDate { get; } = SystemClock.Instance.GetCurrentInstant();

        public Guid CorrelationId { get; internal set; }

        public Dictionary<string, string> Properties { get; } = new();
    }
}