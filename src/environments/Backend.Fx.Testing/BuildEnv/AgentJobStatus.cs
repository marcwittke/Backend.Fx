namespace Backend.Fx.Testing.BuildEnv
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