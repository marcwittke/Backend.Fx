namespace Backend.Fx.Environment.MultiTenancy
{
    /// <summary>
    /// By means of this instance, the IBackendFxApplication gains insight about all active tenants. This is required, when for
    /// example a job
    /// should be executed for all tenants or data should be generated for all tenants during startup.
    /// The <see cref="ITenantService" /> can provide such implementation, but this can only be done in process. When the
    /// tenant service is
    /// running in another process, the implementation must be done using a suitable remoting technology.
    /// </summary>
    public interface ITenantIdProvider
    {
        TenantId[] GetActiveTenantIds();
        TenantId[] GetActiveDemonstrationTenantIds();
        TenantId[] GetActiveProductionTenantIds();
    }
}
