namespace Backend.Fx.Xunit.BuildEnv
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