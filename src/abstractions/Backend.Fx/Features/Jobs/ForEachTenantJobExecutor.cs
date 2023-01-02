using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Features.MultiTenancy;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.Jobs
{
    [UsedImplicitly]
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

            await new ForEachTenantIdInvoker(
                    _tenantEnumerator.GetActiveTenantIds(),
                    tenantWideMutexManager,
                    typeof(TJob).FullName, application.Invoker)
                .InvokeAsync(
                    async (sp, ct) => await sp.GetRequiredService<TJob>().RunAsync(ct).ConfigureAwait(false),
                    identity ?? new SystemIdentity(),
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }
}