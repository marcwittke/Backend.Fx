using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.Jobs
{
    public static class BackendFxApplicationEx
    {
        public static async Task ExecuteJob<TJob>(this IBackendFxApplication application, CancellationToken cancellationToken = default)
            where TJob : IJob
        {
            var jobExecutor = application.CompositionRoot.ServiceProvider.GetRequiredService<IJobExecutor>();
            await jobExecutor.ExecuteAsync<TJob>(application, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}