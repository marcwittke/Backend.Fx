namespace Backend.Fx.Environment.MultiTenancy
{
    public class TenantDeactivated : TenantStatusChanged
    {
        public TenantDeactivated(int tenantId, string name, string description, bool isDemoTenant)
            : base(tenantId, name, description, isDemoTenant)
        {
        }
    }
}