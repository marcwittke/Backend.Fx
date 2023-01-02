namespace Backend.Fx.Features.MultiTenancy
{
    public interface ICurrentTenantIdSelector
    {
        TenantId GetCurrentTenantId();
    }
}