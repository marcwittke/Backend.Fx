using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
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

        public async Task InvokeAsync(Func<IServiceProvider, CancellationToken, Task> awaitableAsyncAction,
            IIdentity identity, CancellationToken cancellationToken = default)
        {
            foreach (TenantId tenantId in _tenantIds)
            {
                var mutex = await _tenantWideMutexManager.TryAcquireAsync(tenantId, _mutexKey, cancellationToken).ConfigureAwait(false);
                if (mutex == null)
                {
                    continue;
                }
                
                try
                {
                    await _invoker.InvokeAsync((sp, ct) =>
                    {
                        sp.GetRequiredService<ICurrentTHolder<TenantId>>().ReplaceCurrent(tenantId);
                        return awaitableAsyncAction(sp, ct);
                    }, identity, cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    mutex.Dispose();
                }
            }
        }
        
        public Task InvokeAsync(Func<IServiceProvider, Task> awaitableAsyncAction, IIdentity identity = null)
            => InvokeAsync((sp, _) => awaitableAsyncAction.Invoke(sp), identity);
    }
}