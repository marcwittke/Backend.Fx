namespace Backend.Fx.Environment.MultiTenancy
{
    public interface ITenantManager
    {
        TenantId[] GetTenantIds();
        Tenant[] GetTenants();
        bool IsActive(TenantId tenantId);
        TenantId CreateDemonstrationTenant(string name, string description);
        TenantId CreateProductionTenant(string name, string description);
        void EnsureTenantIsInitialized(TenantId tenantId);
    }
}
