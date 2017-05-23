namespace Backend.Fx.Patterns.Jobs
{
    /// <summary>
    /// This interface describes a job that can be executed directoy or by a scheduler.
    /// </summary>
    public interface IJob
    {
        /// <summary>
        /// Will be executed in a separate runtime scope
        /// </summary>
        void Execute();
    }
}
