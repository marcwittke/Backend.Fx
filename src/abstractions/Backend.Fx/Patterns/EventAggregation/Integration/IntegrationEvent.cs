using System;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    public interface IIntegrationEvent
    {
        Guid Id { get; }
        DateTime CreationDate { get; }
        int TenantId { get; set; }
        Guid CorrelationId { get; set; }
    }

    /// <summary>
    /// Events that should be handled in a separate context. Might be persisted as well using an external message bus.
    /// See https://blogs.msdn.microsoft.com/cesardelatorre/2017/02/07/domain-events-vs-integration-events-in-domain-driven-design-and-microservices-architectures/
    /// </summary>
    public abstract class IntegrationEvent : IIntegrationEvent
    {
        public Guid Id { get; } = Guid.NewGuid();

        public DateTime CreationDate { get; } = DateTime.UtcNow;
        public int TenantId { get; set; }
        public Guid CorrelationId { get; set; }

        protected IntegrationEvent()
        { }
        
        [Obsolete("TenantId is injected automatically when publishing the event")]
        protected IntegrationEvent(int tenantId)
        {
            TenantId = tenantId;
        }
    }
}