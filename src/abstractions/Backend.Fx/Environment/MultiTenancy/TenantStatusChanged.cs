using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.Environment.MultiTenancy
{
    public abstract class TenantStatusChanged : IntegrationEvent
    {
        protected TenantStatusChanged(string name, string description, bool isDemoTenant)
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