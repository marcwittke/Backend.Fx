namespace DemoBlog.Application.Jobs
{
    using Backend.Fx.Logging;
    using FluentScheduler;
    using IJob = Backend.Fx.Patterns.Jobs.IJob;

    public class JobSchedule : Registry
    {
        public JobSchedule()
        {
            NonReentrantAsDefault();
            
            // NOTE: all application jobs must be wrapped in an ApplicationRuntimeJobWrapper for scheduling. Example:
             Schedule<ApplicationRuntimeJobWrapper<MyCoolJob>>().ToRunNow().AndEvery(30).Seconds();
            
        }
    }

    public class MyCoolJob : IJob
    {
        private static readonly ILogger Logger = LogManager.Create<MyCoolJob>();

        public void Run()
        {
            Logger.Info("Job run");
        }
    }
}