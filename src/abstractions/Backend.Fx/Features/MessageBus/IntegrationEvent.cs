using System;
using Backend.Fx.Environment.MultiTenancy;
using JetBrains.Annotations;

namespace Backend.Fx.Features.MessageBus
{
    [PublicAPI]
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

        public int TenantId { get; private set; }

        public Guid CorrelationId { get; private set; }

        protected IntegrationEvent()
        {
        }

        [Obsolete("TenantId is maintained by the framework now")]
        protected IntegrationEvent(int tenantId)
        {
            TenantId = tenantId;
        }

        internal void SetCorrelationId(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        public void SetTenantId(TenantId tenantId)
        {
            TenantId = (int) tenantId;
        }
    }
}