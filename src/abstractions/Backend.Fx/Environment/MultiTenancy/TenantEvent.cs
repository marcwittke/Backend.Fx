namespace Backend.Fx.Environment.MultiTenancy
{
    public abstract class TenantEvent
    {
        protected TenantEvent(int tenantId, string name, string description, bool isDemoTenant)
        {
            TenantId = tenantId;
            Name = name;
            Description = description;
            IsDemoTenant = isDemoTenant;
        }

        public int TenantId { get; }
        public string Name { get; }

        public string Description { get; }

        public bool IsDemoTenant { get; }
    }
}