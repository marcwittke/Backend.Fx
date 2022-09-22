using System.Security.Principal;
using System.Threading.Tasks;
using Backend.Fx.ExecutionPipeline;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.Jobs
{
    public static class JobsFeature
    {
        /// <summary>
        /// The feature "Jobs" makes sure, that all implementations of <see cref="IJob"/> are injected as scoped instances.
        /// You can use <see cref="RunJob{TJob}"/> to execute any job later on, using a scheduler or other triggers.
        /// </summary>
        /// <param name="application"></param>
        [PublicAPI]
        public static void EnableJobs(IBackendFxApplication application)
        {
            application.CompositionRoot.RegisterModules(new JobModule(application.Assemblies));
        }
        
        /// <summary>
        /// Runs the given job.
        /// </summary>
        /// <param name="application"></param>
        /// <param name="identity">The identity who should run the job. Defaults to <see cref="SystemIdentity"/> when omitted.</param>
        /// <typeparam name="TJob"></typeparam>
        [PublicAPI]
        public static async Task RunJob<TJob>(this IBackendFxApplication application, IIdentity identity = null)
            where TJob : IJob
        {
            identity = identity ?? new SystemIdentity();
            await application
                .Invoker
                .InvokeAsync(async sp =>
                    await sp.GetRequiredService<TJob>().RunAsync().ConfigureAwait(false), identity)
                .ConfigureAwait(false);
        }
    }
}