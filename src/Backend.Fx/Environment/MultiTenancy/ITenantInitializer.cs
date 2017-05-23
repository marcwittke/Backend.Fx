namespace Backend.Fx.Environment.MultiTenancy
{
    public interface ITenantInitializer
    {
        void RunProductiveInitialDataGenerators(TenantId tenantId);
        void RunDemoInitialDataGenerators(TenantId tenantId);
    }
}