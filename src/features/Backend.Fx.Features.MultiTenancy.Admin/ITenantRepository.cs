namespace Backend.Fx.Features.TenantsAdmin
{
    public interface ITenantRepository
    {
        void SaveTenant(Tenant tenant);

        Tenant[] GetTenants();

        Tenant GetTenant(int tenantId);

        void DeleteTenant(int tenantId);
    }
}