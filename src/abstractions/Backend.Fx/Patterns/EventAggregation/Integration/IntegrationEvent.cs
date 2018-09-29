namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    using System;

    public interface IIntegrationEvent
    {
        Guid Id { get; }
        DateTime CreationDate { get; }
        int TenantId { get; }
    }

    /// <summary>
    /// Events that should be handled in a separate context. Might be persisted as well using an external event bus.
    /// See https://blogs.msdn.microsoft.com/cesardelatorre/2017/02/07/domain-events-vs-integration-events-in-domain-driven-design-and-microservices-architectures/
    /// </summary>
    public class IntegrationEvent : IIntegrationEvent
    {
        public Guid Id { get; } = Guid.NewGuid();

        public DateTime CreationDate { get; } = DateTime.UtcNow;

        public int TenantId { get; }

        public IntegrationEvent(int tenantId)
        {
            TenantId = tenantId;
        }
    }
}
