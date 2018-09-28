namespace Backend.Fx.Xunit.BuildEnv
{
    public enum BuildProvider
    {
        None,
        /// <summary>
        ///     TFS Git repository
        /// </summary>
        TfsGit,

        /// <summary>
        ///     Team Foundation Version Control
        /// </summary>
        TfsVersionControl,

        /// <summary>
        ///     Git repository hosted on an external server
        /// </summary>
        Git,

        /// <summary>
        ///     GitHub
        /// </summary>
        GitHub,

        /// <summary>
        ///     Subversion
        /// </summary>
        Svn
    }
}