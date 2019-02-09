namespace Backend.Fx.Patterns.Jobs
{
    using Logging;

    public class JobExecutor<TJob> : IJobExecutor<TJob> where TJob : IJob
    {
        private readonly TJob _job;
        private static readonly ILogger Logger = LogManager.Create<JobExecutor<TJob>>();

        public JobExecutor(TJob job)
        {
            _job = job;
        }

        public void ExecuteJob()
        {
            Logger.Info($"Executing {typeof(TJob).Name}");
            _job.Run();
        }
    }
}