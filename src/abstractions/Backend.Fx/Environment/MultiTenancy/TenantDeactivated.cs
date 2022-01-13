namespace Backend.Fx.Environment.MultiTenancy
{
    public class TenantDeactivated : TenantEvent
    {
        public TenantDeactivated(int tenantId, string name, string description, bool isDemoTenant)
            : base(tenantId, name, description, isDemoTenant)
        {
        }
    }
}