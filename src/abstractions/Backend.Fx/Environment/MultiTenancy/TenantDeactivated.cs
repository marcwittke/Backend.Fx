namespace Backend.Fx.Environment.MultiTenancy
{
    public class TenantDeactivated : TenantStatusChanged
    {
        public TenantDeactivated(string name, string description, bool isDemoTenant)
            : base(name, description, isDemoTenant)
        {
        }
    }
}