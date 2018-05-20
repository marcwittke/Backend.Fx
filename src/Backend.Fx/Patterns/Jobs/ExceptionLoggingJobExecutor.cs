using System;

namespace Backend.Fx.Patterns.Jobs
{
    using System.Threading.Tasks;
    using Logging;

    public class ExceptionLoggingJobExecutor<TJob> : IJobExecutor<TJob>
    {
        private readonly IExceptionLogger exceptionLogger;
        private readonly IJobExecutor<TJob> jobExecutor;

        public ExceptionLoggingJobExecutor(IExceptionLogger exceptionLogger, IJobExecutor<TJob> jobExecutor)
        {
            this.exceptionLogger = exceptionLogger;
            this.jobExecutor = jobExecutor;
        }

        public void ExecuteJob()
        {
            try
            {
                jobExecutor.ExecuteJob();
            } 
            catch(Exception ex)
            {
                exceptionLogger.LogException(ex);
                throw;
            }
        }

        public async Task ExecuteJobAsync()
        {
            try
            {
                await jobExecutor.ExecuteJobAsync();
            } 
            catch(Exception ex)
            {
                exceptionLogger.LogException(ex);
                throw;
            }
        }
    }
}
