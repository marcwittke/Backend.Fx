namespace Backend.Fx.Patterns.Jobs
{
    using System.Threading.Tasks;
    using Logging;
    using UnitOfWork;

    public class UnitOfWorkJobExecutor<TJob> : IJobExecutor<TJob>
    {
        private static readonly ILogger Logger = LogManager.Create<UnitOfWorkJobExecutor<TJob>>();
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJobExecutor<TJob> _jobExecutor;
        
        public UnitOfWorkJobExecutor(IUnitOfWork unitOfWork, IJobExecutor<TJob> jobExecutor)
        {
            Logger.Info($"Beginning unit of work for {typeof(TJob).Name}");
            _unitOfWork = unitOfWork;
            _jobExecutor = jobExecutor;
        }

        public void ExecuteJob()
        {
            try
            {
                _unitOfWork.Begin();
                _jobExecutor.ExecuteJob();
                Logger.Info($"Completing unit of work for {typeof(TJob).Name}");
                _unitOfWork.Complete();
            } 
            catch
            {
                Logger.Info($"Aborting unit of work for {typeof(TJob).Name}");
                throw;
            }
            finally
            {
                _unitOfWork.Dispose();
            }
        }

        public async Task ExecuteJobAsync()
        {
            try
            {
                _unitOfWork.Begin();
                await _jobExecutor.ExecuteJobAsync();
                Logger.Info($"Completing unit of work for {typeof(TJob).Name}");
                _unitOfWork.Complete();
            } 
            catch
            {
                Logger.Info($"Aborting unit of work for {typeof(TJob).Name}");
                throw;
            }
            finally
            {
                _unitOfWork.Dispose();
            }
        }
    }
}
