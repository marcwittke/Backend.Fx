using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.Environment.MultiTenancy
{
    public abstract class TenantEvent : IntegrationEvent
    {
        protected TenantEvent(int tenantId, string name, string description, bool isDemoTenant) : base()
        {
            Name = name;
            Description = description;
            IsDemoTenant = isDemoTenant;
        }

        public string Name { get; }

        public string Description { get; }

        public bool IsDemoTenant { get; }
    }
}