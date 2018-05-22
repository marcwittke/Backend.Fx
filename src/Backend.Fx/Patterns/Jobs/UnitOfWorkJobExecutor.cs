namespace Backend.Fx.Patterns.Jobs
{
    using System.Threading.Tasks;
    using Logging;
    using UnitOfWork;

    public class UnitOfWorkJobExecutor<TJob> : IJobExecutor<TJob>
    {
        private static readonly ILogger Logger = LogManager.Create<UnitOfWorkJobExecutor<TJob>>();
        private readonly IUnitOfWork unitOfWork;
        private readonly IJobExecutor<TJob> jobExecutor;
        
        public UnitOfWorkJobExecutor(IUnitOfWork unitOfWork, IJobExecutor<TJob> jobExecutor)
        {
            Logger.Info($"Beginning unit of work for {typeof(TJob).Name}");
            this.unitOfWork = unitOfWork;
            this.jobExecutor = jobExecutor;
        }

        public void ExecuteJob()
        {
            try
            {
                unitOfWork.Begin();
                jobExecutor.ExecuteJob();
                Logger.Info($"Completing unit of work for {typeof(TJob).Name}");
                unitOfWork.Complete();
            } 
            catch
            {
                Logger.Info($"Aborting unit of work for {typeof(TJob).Name}");
                throw;
            }
            finally
            {
                unitOfWork.Dispose();
            }
        }

        public async Task ExecuteJobAsync()
        {
            try
            {
                unitOfWork.Begin();
                await jobExecutor.ExecuteJobAsync();
                Logger.Info($"Completing unit of work for {typeof(TJob).Name}");
                unitOfWork.Complete();
            } 
            catch
            {
                Logger.Info($"Aborting unit of work for {typeof(TJob).Name}");
                throw;
            }
            finally
            {
                unitOfWork.Dispose();
            }
        }
    }
}
