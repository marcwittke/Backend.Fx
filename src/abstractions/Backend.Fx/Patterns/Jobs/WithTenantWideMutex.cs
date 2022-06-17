using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Patterns.Jobs
{
    [PublicAPI]
    public class WithTenantWideMutex<TJob> : IJob where TJob : IJob
    {
        private static readonly ILogger Logger = Log.Create<WithTenantWideMutex<TJob>>();
        private readonly TJob _job;
        private readonly ICurrentTHolder<TenantId> _tenantIdHolder;
        private readonly ITenantWideMutexManager _tenantWideMutexManager;

        public WithTenantWideMutex(
            ITenantWideMutexManager tenantWideMutexManager,
            TJob job,
            ICurrentTHolder<TenantId> tenantIdHolder)
        {
            _tenantWideMutexManager = tenantWideMutexManager;
            _job = job;
            _tenantIdHolder = tenantIdHolder;
        }

        public void Run()
        {
            if (_tenantWideMutexManager.TryAcquire(_tenantIdHolder.Current, typeof(TJob).Name, out var mutex))
            {
                try
                {
                    _job.Run();
                }
                finally
                {
                    mutex.Dispose();
                }
            }
            else
            {
                var tenantIdString
                    = _tenantIdHolder.Current.HasValue ? _tenantIdHolder.Current.Value.ToString() : "null";
                Logger.LogInformation("{Job} is already running in tenant {TenantId}", typeof(TJob).Name, tenantIdString);
            }
        }
    }
}