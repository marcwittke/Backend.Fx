namespace Backend.Fx.Environment.MultiTenancy
{
    public class TenantDeleted : TenantEvent
    {
        public TenantDeleted(int tenantId, string name, string description, bool isDemoTenant)
            : base(tenantId, name, description, isDemoTenant)
        {
        }
    }
}