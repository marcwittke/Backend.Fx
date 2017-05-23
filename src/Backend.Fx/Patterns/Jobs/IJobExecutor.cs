namespace Backend.Fx.Patterns.Jobs
{
    public interface IJobExecutor
    {
        void ExecuteJob<TJob>(int? tenantId = null, int delayInSeconds = 0) where TJob : class, IJob;
    }
}
