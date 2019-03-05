namespace Backend.Fx.Environment.MultiTenancy
{
    public interface ITenantRepository
    {
        void SaveTenant(Tenant tenant);

        Tenant[] GetTenants();

        Tenant GetTenant(TenantId tenantId);
    }
}