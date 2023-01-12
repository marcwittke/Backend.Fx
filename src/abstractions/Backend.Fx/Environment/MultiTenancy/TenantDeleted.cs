namespace Backend.Fx.Environment.MultiTenancy
{
    public class TenantDeleted : TenantEvent
    {
        public TenantDeleted(string name, string description, bool isDemoTenant)
            : base(name, description, isDemoTenant)
        {
        }
    }
}