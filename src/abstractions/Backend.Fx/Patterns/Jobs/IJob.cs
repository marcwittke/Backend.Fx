namespace Backend.Fx.Patterns.Jobs
{
    /// <summary>
    /// This interface describes a job that can be executed directly or by a scheduler.
    /// </summary>
    public interface IJob
    {
        void Run();
    }
}