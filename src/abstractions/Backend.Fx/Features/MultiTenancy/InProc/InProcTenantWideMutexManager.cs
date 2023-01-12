using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Backend.Fx.Features.MultiTenancy.InProc
{
    /// <summary>
    /// If an instance of this class is being hold as singleton it can be used to manage tenant wide
    /// mutexes in a single process. When multiple processes are running, another locking mechanism
    /// must be implemented (e.g. using MS SQL`s <code>sp_getapplock</code> or Postgres` advisory locks)  
    /// </summary>
    public class InProcTenantWideMutexManager : ITenantWideMutexManager
    {
        private readonly ConcurrentDictionary<TenantId, ConcurrentDictionary<string, SemaphoreSlim>> _semaphoresByTenantId = new();

        public async Task<ITenantWideMutex> TryAcquireAsync(TenantId tenantId, string key, CancellationToken cancellationToken)
        {
            if (!tenantId.HasValue)
            {
                throw new InvalidOperationException("Cannot acquire a tenant wide lock for tenant NULL");
            }


            var subDictionary = _semaphoresByTenantId.GetOrAdd(
                tenantId,
                _ => new ConcurrentDictionary<string, SemaphoreSlim>(new[] { new KeyValuePair<string, SemaphoreSlim>(key, new SemaphoreSlim(1)) }));
            
            var semaphore = subDictionary.GetOrAdd(
                key, 
                _ => new SemaphoreSlim(1));
            
            if (await semaphore.WaitAsync(300, cancellationToken).ConfigureAwait(false))
            {
                return new InProcTenantWideMutex(() => semaphore.Release());
            }

            return null;
        }

        private class InProcTenantWideMutex : ITenantWideMutex
        {
            private readonly Action _dispose;

            public InProcTenantWideMutex([NotNull] Action dispose)
            {
                _dispose = dispose ?? throw new ArgumentNullException(nameof(dispose));
            }

            public void Dispose()
            {
                _dispose.Invoke();
            }
        }
    }
}