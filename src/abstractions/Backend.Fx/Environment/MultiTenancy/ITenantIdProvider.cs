namespace Backend.Fx.Environment.MultiTenancy
{
    /// <summary>
    /// By means of this instance, other services gain insight about all active tenants. This is useful, when for example a job
    /// should be executed for all tenants or data should be generated for all tenants during startup.
    /// </summary>
    public interface ITenantIdProvider
    {
        TenantId[] GetActiveTenantIds();
        TenantId[] GetActiveDemonstrationTenantIds();
        TenantId[] GetActiveProductionTenantIds();
    }
}