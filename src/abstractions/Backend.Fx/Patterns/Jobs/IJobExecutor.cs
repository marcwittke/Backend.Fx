namespace Backend.Fx.Patterns.Jobs
{
    /// <summary>
    /// Abstracts the functionality of the framework to run a job in background. This can be triggered 
    /// by a scheduler, or, also be done manually
    /// </summary>
    public interface IJobExecutor
    {
        void ExecuteJob();
    }

    // ReSharper disable once UnusedTypeParameter : this interface enables the use of decorators
    public interface IJobExecutor<TJob> : IJobExecutor {}
}
