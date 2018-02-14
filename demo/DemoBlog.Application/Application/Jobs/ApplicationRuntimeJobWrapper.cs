namespace DemoBlog.Bootstrapping.Jobs
{
    using Backend.Fx.Patterns.Jobs;
    using JetBrains.Annotations;

    /// <summary>
    /// Wraps a pure Application Job (implementing <see cref="FluentScheduler.IJob"/>) in a 
    /// Fluent Scheduler Job (implementing <see cref="FluentScheduler.IJob"/>) to make it schedulable
    /// </summary>
    public class ApplicationRuntimeJobWrapper<TApplicationJob> : FluentScheduler.IJob where TApplicationJob : class, Backend.Fx.Patterns.Jobs.IJob
    {
        private readonly IJobExecutor jobExecutor;

        [UsedImplicitly]
        public ApplicationRuntimeJobWrapper(IJobExecutor jobExecutor)
        {
            this.jobExecutor = jobExecutor;
        }

        [UsedImplicitly]
        public void Execute()
        {
            jobExecutor.ExecuteJobAsync<TApplicationJob>().RunSynchronously();
        }
    }
}