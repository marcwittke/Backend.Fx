namespace Backend.Fx.Testing.BuildEnv
{
    public static class Build
    {
        /// <summary>
        /// Set to True if the script is being run by a build step.
        /// </summary>
        public static bool IsTfBuild => global::System.Environment.GetEnvironmentVariable("TF_BUILD") == "True";

        /// <summary>
        ///     The local path on the agent where any artifacts are copied to before being pushed to their destination. For
        ///     example: c:\agent\_work\1\a
        ///     A typical way to use this folder is to publish your build artifacts with the Copy files and Publish build artifacts
        ///     steps.
        /// </summary>
        /// <remarks>
        ///     Build.ArtifactStagingDirectory and Build.StagingDirectory are interchangeable. This directory is purged before
        ///     each new build, so you don't have to clean it up yourself.
        /// </remarks>
        public static string ArtifactStagingDirectory => global::System.Environment.GetEnvironmentVariable("BUILD_ARTIFACTSTAGINGDIRECTORY");

        /// <summary>
        ///     The ID of the record for the completed build.
        /// </summary>
        public static string Id => global::System.Environment.GetEnvironmentVariable("BUILD_BUILDID");

        /// <summary>
        ///     The name of the completed build. You can specify the build number format that generates this value in the
        ///     definition options.
        ///     A typical use of this variable is to make it part of the label format, which you specify on the repository tab.
        ///     Note: This value can contain whitespace or other invalid label characters. In these cases, the label format will
        ///     fail.
        /// </summary>
        public static string Number => global::System.Environment.GetEnvironmentVariable("BUILD_BUILDNUMBER");

        /// <summary>
        ///     The URI for the build. For example: vstfs:///Build/Build/1430.
        /// </summary>
        public static string Uri => global::System.Environment.GetEnvironmentVariable("BUILD_BUILDURI");

        /// <summary>
        ///     The local path on the agent you can use as an output folder for compiled binaries.
        ///     For example: c:\agent\_work\1\b.
        ///     By default, new build definitions are not set up to clean this directory. You can define your build to clean it up
        ///     on the Repository tab.
        /// </summary>
        public static string BinariesDirectory => global::System.Environment.GetEnvironmentVariable("BUILD_BINARIESDIRECTORY");

        /// <summary>
        ///     The name of the build definition.
        ///     Note: This value can contain whitespace or other invalid label characters. In these cases, the label format will
        ///     fail.
        /// </summary>
        public static string DefinitionName => global::System.Environment.GetEnvironmentVariable("BUILD_DEFINITIONNAME");

        /// <summary>
        ///     The version of the build definition.
        /// </summary>
        public static string DefinitionVersion => global::System.Environment.GetEnvironmentVariable("BUILD_DEFINITIONVERSION");

        public static string QueuedBy => global::System.Environment.GetEnvironmentVariable("BUILD_QUEUEDBY");

        /// <summary>
        ///     The event that caused the build to run.
        /// </summary>
        public static BuildReason Reason
        {
            get
            {
                var buildReasonString = global::System.Environment.GetEnvironmentVariable("BUILD_REASON") ?? "None";
                return (BuildReason) global::System.Enum.Parse(typeof(BuildReason), buildReasonString);
            }
        }

        /// <summary>
        ///     The value you've selected for Clean in the source repository settings.
        /// </summary>
        public static string RepositoryClean => global::System.Environment.GetEnvironmentVariable("BUILD_REPOSITORY_CLEAN");

        /// <summary>
        ///     The local path on the agent where your source code files are downloaded. For example: c:\agent\_work\1\s
        ///     By default, new build definitions update only the changed files. You can modify how files are downloaded on the
        ///     Repository tab.
        /// </summary>
        public static string LocalPath => global::System.Environment.GetEnvironmentVariable("BUILD_REPOSITORY_LOCALPATH");

        /// <summary>
        ///     The name of the repository.
        /// </summary>
        public static string RepositoryName => global::System.Environment.GetEnvironmentVariable("BUILD_REPOSITORY_NAME");

        /// <summary>
        ///     The type of repository you selected.
        /// </summary>
        public static BuildProvider Provider
        {
            get
            {
                var environmentVariable = global::System.Environment.GetEnvironmentVariable("BUILD_REPOSITORY_PROVIDER") ?? "None";
                return (BuildProvider) global::System.Enum.Parse(typeof(BuildProvider), environmentVariable);
            }
        }

        /// <summary>
        ///     The URL for the repository. For example:
        ///     Git: https://fabrikamfiber.visualstudio.com/_git/Scripts
        ///     TFVC: https://fabrikamfiber.visualstudio.com/
        /// </summary>
        public static string RepositoryUri => global::System.Environment.GetEnvironmentVariable("BUILD_REPOSITORY_URI");

        public static string RequestedFor => global::System.Environment.GetEnvironmentVariable("BUILD_REQUESTEDFOR");

        public static string RequestedForEmail => global::System.Environment.GetEnvironmentVariable("BUILD_REQUESTEDFOREMAIL");

        public static string RequestedForId => global::System.Environment.GetEnvironmentVariable("BUILD_REQUESTEDFORID");

        /// <summary>
        ///     The branch the build was queued for. Some examples:
        ///     Git repo branch: refs/heads/master
        ///     Git repo pull request: refs/pull/1/merge
        ///     TFVC repo branch: $/teamproject/main
        ///     TFVC repo gated check-in: Gated_2016-06-06_05.20.51.4369;username@live.com
        ///     TFVC repo shelveset build: myshelveset;username@live.com
        /// </summary>
        public static string SourceBranch => global::System.Environment.GetEnvironmentVariable("BUILD_SOURCEBRANCH");

        /// <summary>
        ///     The name of the branch the build was queued for.
        ///     Git repo branch or pull request: The last path segment in the ref. For example, in refs/heads/master this value is
        ///     master.
        ///     TFVC repo branch: The last path segment in the root server path for the workspace. For example in
        ///     $/teamproject/main this value is main.
        ///     TFVC repo gated check-in or shelveset build is the name of the shelveset. For example,
        ///     Gated_2016-06-06_05.20.51.4369;username@live.com or myshelveset;username@live.com.
        ///     Note: In TFVC, if you are running a gated check-in build or manually building a shelveset, you cannot use this
        ///     variable in your build number format.
        /// </summary>
        public static string SourceBranchName => global::System.Environment.GetEnvironmentVariable("BUILD_SOURCEBRANCHNAME");

        /// <summary>
        ///     The local path on the agent where your source code files are downloaded. For example: c:\agent\_work\1\s
        ///     By default, new build definitions update only the changed files. You can modify how files are downloaded on the
        ///     Repository tab.
        /// </summary>
        public static string SourcesDirectory => global::System.Environment.GetEnvironmentVariable("BUILD_SOURCESDIRECTORY");

        /// <summary>
        ///     The latest version control change that is included in this build.
        ///     Git: The commit ID.
        ///     TFVC: the changeset.
        /// </summary>
        public static string SourceVersion => global::System.Environment.GetEnvironmentVariable("BUILD_SOURCEVERSION");

        /// <summary>
        ///     The local path on the agent where any artifacts are copied to before being pushed to their destination. For
        ///     example: c:\agent\_work\1\a
        ///     A typical way to use this folder is to publish your build artifacts with the Copy files and Publish build artifacts
        ///     steps.
        /// </summary>
        /// <remarks>
        ///     Build.ArtifactStagingDirectory and Build.StagingDirectory are interchangeable. This directory is purged before
        ///     each new build, so you don't have to clean it up yourself.
        /// </remarks>
        public static string StagingDirectory => global::System.Environment.GetEnvironmentVariable("BUILD_STAGINGDIRECTORY");

        /// <summary>
        ///     The value you've selected for Checkout submodules on the repository tab.
        /// </summary>
        public static string RepositoryGitSubModuleCheckout => global::System.Environment.GetEnvironmentVariable("BUILD_REPOSITORY_GIT_SUBMODULECHECKOUT");

        /// <summary>
        ///     Defined if your repository is Team Foundation Version Control.
        ///     If you are running a gated build or a shelveset build, this is set to the name of the shelveset you are building.
        ///     Note: This variable yields a value that is invalid for build use in a build number format
        /// </summary>
        public static string SourceTfvcShelveSet => global::System.Environment.GetEnvironmentVariable("BUILD_SOURCETFVCSHELVESET");

        /// <summary>
        ///     The local path on the agent where the test results are created. For example: c:\agent\_work\1\TestResults
        /// </summary>
        public static string TestResultsDirectory => global::System.Environment.GetEnvironmentVariable("COMMON_TESTRESULTSDIRECTORY");

        /// <summary>
        ///     If the build was triggered by another build, then this variable is set to the BuildID of the triggering build.
        /// </summary>
        public static string TriggeredByBuildId => global::System.Environment.GetEnvironmentVariable("BUILD_TRIGGEREDBY_BUILDID");

        /// <summary>
        ///     If the build was triggered by another build, then this variable is set to the DefinitionID of the triggering build.
        /// </summary>
        public static string TriggeredByBuildDefinitionId => global::System.Environment.GetEnvironmentVariable("BUILD_TRIGGEREDBY_DEFINITIONID");

        /// <summary>
        ///     If the build was triggered by another build, then this variable is set to the name of the triggering build
        ///     definition.
        /// </summary>
        public static string TriggeredByBuildDefinitionName => global::System.Environment.GetEnvironmentVariable("BUILD_TRIGGEREDBY_DEFINITIONNAME");

        /// <summary>
        ///     If the build was triggered by another build, then this variable is set to the number of the triggering build.
        /// </summary>
        public static string TriggeredByBuildNumber => global::System.Environment.GetEnvironmentVariable("BUILD_TRIGGEREDBY_BUILDNUMBER");

        /// <summary>
        ///     If the build was triggered by another build, then this variable is set to ID of the team project that contains the
        ///     triggering build.
        /// </summary>
        public static string TriggeredByProjectId => global::System.Environment.GetEnvironmentVariable("BUILD_TRIGGEREDBY_PROJECTID");
    }
}