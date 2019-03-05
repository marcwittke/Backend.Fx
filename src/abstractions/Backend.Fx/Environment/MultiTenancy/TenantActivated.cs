namespace Backend.Fx.Environment.MultiTenancy
{
    public class TenantActivated : TenantStatusChanged
    {
        public TenantActivated(int tenantId, string name, string description, bool isDemoTenant, string defaultCultureName) 
            : base(tenantId, name, description, isDemoTenant, defaultCultureName)
        {
        }
    }
}