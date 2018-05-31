namespace Backend.Fx.Testing.BuildEnv
{

    public static class Agent
    {
        /// <summary>
        ///     The local path on the agent where all folders for a given build definition are created.
        /// </summary>
        public static string BuildDirectory
        {
            get { return global::System.Environment.GetEnvironmentVariable("AGENT_BUILDDIRECTORY"); }
        }

        /// <summary>
        ///     The directory the agent is installed into. This contains the agent software.
        /// </summary>
        public static string HomeDirectory
        {
            get { return global::System.Environment.GetEnvironmentVariable("AGENT_HOMEDIRECTORY"); }
        }

        /// <summary>
        ///     The ID of the agent.
        /// </summary>
        public static string Id
        {
            get { return global::System.Environment.GetEnvironmentVariable("AGENT_ID"); }
        }

        /// <summary>
        ///     The status of the build.
        /// </summary>
        public static AgentJobStatus JobStatus
        {
            get { return (AgentJobStatus) global::System.Enum.Parse(typeof(AgentJobStatus), global::System.Environment.GetEnvironmentVariable("AGENT_JOBSTATUS") ?? "None"); }
        }

        /// <summary>
        ///     The name of the machine on which the agent is installed.
        /// </summary>
        public static string MachineName
        {
            get { return global::System.Environment.GetEnvironmentVariable("AGENT_MACHINENAME"); }
        }

        /// <summary>
        ///     The name of the agent that is registered with the pool.
        ///     If you are using an on-premises agent, this directory is specified by you.
        /// </summary>
        public static string Name
        {
            get { return global::System.Environment.GetEnvironmentVariable("AGENT_NAME"); }
        }

        /// <summary>
        ///     The working directory for this agent. For example: c:\agent\_work.
        /// </summary>
        public static string WorkFolder
        {
            get { return global::System.Environment.GetEnvironmentVariable("AGENT_WORKFOLDER"); }
        }
    }
}