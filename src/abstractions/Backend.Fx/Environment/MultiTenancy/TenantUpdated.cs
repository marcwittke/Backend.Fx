namespace Backend.Fx.Environment.MultiTenancy
{
    public class TenantUpdated : TenantEvent
    {
        public TenantUpdated(int tenantId, string name, string description, bool isDemoTenant)
            : base(tenantId, name, description, isDemoTenant)
        {
        }
    }
}