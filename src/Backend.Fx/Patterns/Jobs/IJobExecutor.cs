namespace Backend.Fx.Patterns.Jobs
{
    using System.Threading.Tasks;

    /// <summary>
    /// Abstracts the functionality of the framework to run a job in background. This can be triggered 
    /// by a schedulter, or, also be done manually
    /// </summary>
    public interface IJobExecutor
    {
        void ExecuteJob();
        Task ExecuteJobAsync();
    }

    // ReSharper disable once UnusedTypeParameter : this interface enables the use of decorators
    public interface IJobExecutor<TJob> : IJobExecutor {}
}
