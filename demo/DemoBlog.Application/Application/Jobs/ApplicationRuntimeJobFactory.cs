namespace DemoBlog.Bootstrapping.Jobs
{
    using System;
    using Backend.Fx.Logging;
    using Backend.Fx.Patterns.Jobs;
    using FluentScheduler;
    using IJob = FluentScheduler.IJob;

    /// <summary>
    /// Factory to enable FluentScheduler to use our application runtime
    /// </summary>
    public class ApplicationRuntimeJobFactory : IJobFactory
    {
        private static readonly ILogger Logger = LogManager.Create<ApplicationRuntimeJobFactory>();
        private readonly IJobExecutor jobExecutor;

        public ApplicationRuntimeJobFactory(IJobExecutor jobExecutor)
        {
            this.jobExecutor = jobExecutor;
        }

        public IJob GetJobInstance<T>() where T : IJob
        {
            object jobInstance;
            try
            {
                jobInstance = Activator.CreateInstance(typeof(T), jobExecutor);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Unable to create an instance of {typeof(T).Name}. Are you sure it implements ApplicationRuntimeJobWrapper<T>?");
                throw;
            }
            return (IJob)jobInstance;
        }
    }
}