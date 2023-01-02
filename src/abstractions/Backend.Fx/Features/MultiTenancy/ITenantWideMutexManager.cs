namespace Backend.Fx.Features.MultiTenancy
{
    public interface ITenantWideMutexManager
    {
        bool TryAcquire(TenantId tenantId, string key, out ITenantWideMutex mutex);
    }
}