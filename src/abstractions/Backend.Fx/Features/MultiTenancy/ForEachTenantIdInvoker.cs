using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.MultiTenancy
{
    public class ForEachTenantIdInvoker : IBackendFxApplicationInvoker
    {
        private readonly IEnumerable<TenantId> _tenantIds;
        private readonly ITenantWideMutexManager _tenantWideMutexManager;
        private readonly string _mutexKey;
        private readonly IBackendFxApplicationInvoker _invoker;

        public ForEachTenantIdInvoker(
            IEnumerable<TenantId> tenantIds,
            ITenantWideMutexManager tenantWideMutexManager,
            string mutexKey,
            IBackendFxApplicationInvoker invoker)
        {
            _tenantIds = tenantIds;
            _tenantWideMutexManager = tenantWideMutexManager;
            _mutexKey = mutexKey;
            _invoker = invoker;
        }

        public async Task InvokeAsync(Func<IServiceProvider, Task> awaitableAsyncAction, IIdentity identity)
        {
            foreach (var tenantId in _tenantIds)
            {
                if (_tenantWideMutexManager.TryAcquire(tenantId, _mutexKey, out var mutex))
                {
                    try
                    {
                        await _invoker.InvokeAsync(sp =>
                        {
                            sp.GetRequiredService<ICurrentTHolder<TenantId>>().ReplaceCurrent(tenantId);
                            return awaitableAsyncAction(sp);
                        }, identity).ConfigureAwait(false);
                    }
                    finally
                    {
                        mutex.Dispose();
                    }
                }
            }
        }
    }
}