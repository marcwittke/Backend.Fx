using System;
using Backend.Fx.Environment.MultiTenancy;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    public abstract class EventProcessingContext
    {
        public abstract TenantId TenantId { get; }
        public abstract dynamic DynamicEvent { get; }
        public abstract IIntegrationEvent GetTypedEvent(Type eventType);
    }
}