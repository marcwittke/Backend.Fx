using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.Jobs
{
    public interface IJobExecutor
    {
        Task ExecuteAsync<TJob>(IBackendFxApplication application,
            IIdentity identity = null,
            CancellationToken cancellationToken = default)
            where TJob : IJob;
    }

    [UsedImplicitly]
    public class JobExecutor : IJobExecutor
    {
        public async Task ExecuteAsync<TJob>(IBackendFxApplication application,
            IIdentity identity = null,
            CancellationToken cancellationToken = default)
            where TJob : IJob
        {
            await application
                .Invoker
                .InvokeAsync(async (sp, ct) =>
                        await sp
                            .GetRequiredService<TJob>()
                            .RunAsync(ct)
                            .ConfigureAwait(false)
                    , identity, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}