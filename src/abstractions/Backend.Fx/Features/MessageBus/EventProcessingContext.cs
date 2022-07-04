using System;
using Backend.Fx.Environment.MultiTenancy;

namespace Backend.Fx.Features.MessageBus
{
    public abstract class EventProcessingContext
    {
        public abstract TenantId TenantId { get; }
        public abstract dynamic DynamicEvent { get; }
        public abstract Guid CorrelationId { get; }

        public abstract IIntegrationEvent GetTypedEvent(Type eventType);
    }
}