using System;
using System.Collections.Concurrent;
using System.Threading;
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
        private readonly ConcurrentDictionary<int, ConcurrentDictionary<string, Mutex>> _mutexes = new();

        public bool TryAcquire(TenantId tenantId, string key, out ITenantWideMutex tenantWideMutex)
        {
            if (!tenantId.HasValue)
            {
                throw new InvalidOperationException("Cannot acquire a tenant wide lock for tenant NULL");
            }

            lock (this)
            {
                var subDictionary = _mutexes.GetOrAdd(tenantId.Value, _ => new ConcurrentDictionary<string, Mutex>());
                Mutex mutex = subDictionary.GetOrAdd(key, _ => new Mutex());
                if (mutex.WaitOne(300))
                {
                    tenantWideMutex = new InProcTenantWideMutex(() => mutex.ReleaseMutex());
                    return true;
                }

                tenantWideMutex = null;
                return false;
            }
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