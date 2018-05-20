namespace Backend.Fx.Patterns.Jobs
{
    using System.Threading.Tasks;
    using Logging;

    public class JobExecutor<TJob> : IJobExecutor<TJob> where TJob : IJob
    {
        private readonly TJob job;
        private static readonly ILogger Logger = LogManager.Create<JobExecutor<TJob>>();

        public JobExecutor(TJob job)
        {
            this.job = job;
        }

        public void ExecuteJob()
        {
            Logger.Info($"Executing {typeof(TJob).Name}");
            job.Run();
        }

        public async Task ExecuteJobAsync()
        {
            Logger.Info($"Starting {typeof(TJob).Name} asynchronously");
            await Task.Run(() => job.Run());
        }
    }
}