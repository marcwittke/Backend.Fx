namespace Backend.Fx.Patterns.Jobs
{
    using System.Threading.Tasks;
    using Logging;

    public class JobExecutor<TJob> : IJobExecutor<TJob> where TJob : IJob
    {
        private readonly TJob _job;
        private static readonly ILogger Logger = LogManager.Create<JobExecutor<TJob>>();

        public JobExecutor(TJob job)
        {
            this._job = job;
        }

        public void ExecuteJob()
        {
            Logger.Info($"Executing {typeof(TJob).Name}");
            _job.Run();
        }

        public async Task ExecuteJobAsync()
        {
            Logger.Info($"Starting {typeof(TJob).Name} asynchronously");
            await Task.Run(() => _job.Run());
        }
    }
}