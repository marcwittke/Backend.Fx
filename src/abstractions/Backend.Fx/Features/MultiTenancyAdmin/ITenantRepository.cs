namespace Backend.Fx.Features.MultiTenancyAdmin
{
    public interface ITenantRepository
    {
        void SaveTenant(Tenant tenant);

        Tenant[] GetTenants();

        Tenant GetTenant(int tenantId);

        void DeleteTenant(int tenantId);
        
        int GetNextTenantId();
    }
}