using System;
using System.Collections.Concurrent;
using System.Threading;
using JetBrains.Annotations;

namespace Backend.Fx.Features.MultiTenancy
{
    /// <summary>
    /// If an instance of this class is being hold as singleton it can be used to manage tenant wide
    /// mutexes in a single process. When multiple processes are running, another locking mechanism
    /// must be implemented (e.g. using MS SQL`s <code>sp_getapplock</code> or Postgres` advisory locks)  
    /// </summary>
    public class InMemoryTenantWideMutexManager : ITenantWideMutexManager
    {
        private readonly ConcurrentDictionary<int, ConcurrentDictionary<string,Mutex>> _mutexes =
            new ConcurrentDictionary<int, ConcurrentDictionary<string,Mutex>>();
        
        public bool TryAcquire(TenantId tenantId, string key, out ITenantWideMutex tenantWideMutex)
        {
            if (!tenantId.HasValue)
            {
                throw new InvalidOperationException("Cannot acquire a tenant wide lock for tenant NULL");
            }

            lock (this)
            {
                var subDictionary = _mutexes.GetOrAdd(tenantId.Value, i => new ConcurrentDictionary<string, Mutex>());
                var mutex = subDictionary.GetOrAdd(key, s => new Mutex());
                if (mutex.WaitOne(300))
                {
                    tenantWideMutex = new InMemoryTenantWideMutex(() => mutex.ReleaseMutex());
                    return true;
                }

                tenantWideMutex = null;
                return false;
            }
        }
        
        private class InMemoryTenantWideMutex  : ITenantWideMutex
        {
            private readonly Action _dispose;

            public InMemoryTenantWideMutex([NotNull] Action dispose)
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