using System;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    public interface IIntegrationEvent
    {
        Guid Id { get; }
        DateTime CreationDate { get; }
        int TenantId { get; }
        Guid CorrelationId { get; }
    }

    /// <summary>
    /// Events that should be handled in a separate context. Might be persisted as well using an external message bus.
    /// See https://blogs.msdn.microsoft.com/cesardelatorre/2017/02/07/domain-events-vs-integration-events-in-domain-driven-design-and-microservices-architectures/
    /// </summary>
    public abstract class IntegrationEvent : IIntegrationEvent
    {
        public Guid Id { get; } = Guid.NewGuid();

        public DateTime CreationDate { get; } = DateTime.UtcNow;

        public int TenantId { get; }
        
        public Guid CorrelationId { get; private set; }
        
        internal void SetCorrelationId(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        protected IntegrationEvent(int tenantId)
        {
            TenantId = tenantId;
        }
    }
}
