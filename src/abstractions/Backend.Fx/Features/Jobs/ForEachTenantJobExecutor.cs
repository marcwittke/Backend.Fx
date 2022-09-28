using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Features.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.Jobs
{
    public class ForEachTenantJobExecutor : IJobExecutor
    {
        private readonly ITenantEnumerator _tenantEnumerator;
        
        public ForEachTenantJobExecutor(
            ITenantEnumerator tenantEnumerator,
            IJobExecutor _)
        {
            _tenantEnumerator = tenantEnumerator;
        }

        public async Task ExecuteAsync<TJob>(
            IBackendFxApplication application,
            IIdentity identity = null,
            CancellationToken cancellationToken = default)
            where TJob : IJob
        {
            var tenantWideMutexManager =
                application.CompositionRoot.ServiceProvider.GetRequiredService<ITenantWideMutexManager>();
            
            await new ForEachTenantIdInvoker(_tenantEnumerator.GetActiveTenantIds(), tenantWideMutexManager, typeof(TJob).FullName, application.Invoker).InvokeAsync(
                async sp => await sp.GetRequiredService<TJob>().RunAsync(cancellationToken).ConfigureAwait(false),
                identity ?? new SystemIdentity()).ConfigureAwait(false);
        }
    }
}