namespace Backend.Fx.Environment.MultiTenancy
{
    public class TenantActivated : TenantEvent
    {
        public TenantActivated(string name, string description, bool isDemoTenant) 
            : base(name, description, isDemoTenant)
        {
        }
    }
}