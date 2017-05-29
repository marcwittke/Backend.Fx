namespace Backend.Fx.Patterns.Jobs
{
    using System.Threading.Tasks;

    public interface IJobExecutor
    {
        Task ExecuteJobAsync<TJob>(int? tenantId = null, int delayInSeconds = 0) where TJob : class, IJob;
    }
}
