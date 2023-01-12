using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.Features.MultiTenancy
{
    public interface ITenantWideMutexManager
    {
        Task<ITenantWideMutex> TryAcquireAsync(TenantId tenantId, string key, CancellationToken cancellationToken);
    }
}