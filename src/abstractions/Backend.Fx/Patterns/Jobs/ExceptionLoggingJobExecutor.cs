namespace Backend.Fx.Patterns.Jobs
{
    using System;
    using Logging;

    public class ExceptionLoggingJobExecutor<TJob> : IJobExecutor<TJob>
    {
        private readonly IExceptionLogger _exceptionLogger;
        private readonly IJobExecutor<TJob> _jobExecutor;

        public ExceptionLoggingJobExecutor(IExceptionLogger exceptionLogger, IJobExecutor<TJob> jobExecutor)
        {
            _exceptionLogger = exceptionLogger;
            _jobExecutor = jobExecutor;
        }

        public void ExecuteJob()
        {
            try
            {
                _jobExecutor.ExecuteJob();
            } 
            catch(Exception ex)
            {
                _exceptionLogger.LogException(ex);
                throw;
            }
        }
    }
}
