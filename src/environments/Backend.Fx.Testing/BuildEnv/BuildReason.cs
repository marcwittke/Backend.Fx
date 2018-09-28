namespace Backend.Fx.Testing.BuildEnv
{
    public enum BuildReason
    {
        None,
        /// <summary>
        ///     A user manually queued the build.
        /// </summary>
        Manual,

        /// <summary>
        ///     Continuous integration (CI) triggered by a Git push or a TFVC check-in.
        /// </summary>
        IndividualCi,

        /// <summary>
        ///     Continuous integration (CI) triggered by a Git push or a TFVC check-in, and the Batch changes was selected.
        /// </summary>
        BatchedCi,

        /// <summary>
        ///     Scheduled trigger.
        /// </summary>
        Schedule,

        /// <summary>
        ///     A user manually queued the build of a specific TFVC shelve set.
        /// </summary>
        ValidateShelveset,

        /// <summary>
        ///     Gated check-in trigger.
        /// </summary>
        CheckInShelveset,

        /// <summary>
        ///     The build was triggered by a Git branch policy that requires a build.
        /// </summary>
        PullRequest,

        /// <summary>
        ///     The build was triggered by another build.
        /// </summary>
        BuildCompletion
    }
}