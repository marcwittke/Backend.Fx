namespace DemoBlog.Bootstrapping.Jobs
{
    using FluentScheduler;

    public class JobSchedule : Registry
    {
        public JobSchedule()
        {
            NonReentrantAsDefault();
            
            // NOTE: all application jobs must be wrapped in an ApplicationRuntimeJobWrapper for scheduling. Example:
            // Schedule<ApplicationRuntimeJobWrapper<MyCoolJob>>().Every(60).Minutes();

            
            // add more jobs here
        }
    }
}