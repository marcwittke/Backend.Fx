namespace Backend.Fx.Tests.Environment
{
    public enum AgentJobStatus
    {
        None,
        Canceled,
        Failed,
        Succeeded,

        /// <summary>
        ///     partially successful
        /// </summary>
        SucceededWithIssues
    }
}