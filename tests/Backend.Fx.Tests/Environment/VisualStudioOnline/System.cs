namespace Backend.Fx.Tests.Environment.VisualStudioOnline
{
    public static class System
    {
        /// <summary>
        ///     The OAuth token to access the REST API.
        /// </summary>
        public static string AccessToken
        {
            get { return global::System.Environment.GetEnvironmentVariable("SYSTEM_ACCESSTOKEN"); }
        }

        /// <summary>
        ///     The GUID of the team foundation collection.
        /// </summary>
        public static string CollectionId
        {
            get { return global::System.Environment.GetEnvironmentVariable("SYSTEM_COLLECTIONID"); }
        }

        /// <summary>
        ///     The local path on the agent where your source code files are downloaded. For example: c:\agent\_work\1\s
        ///     By default, new build definitions update only the changed files. You can modify how files are downloaded on the
        ///     Repository tab.
        /// </summary>
        public static string DefaultWorkingDirectory
        {
            get { return global::System.Environment.GetEnvironmentVariable("SYSTEM_DEFAULTWORKINGDIRECTORY"); }
        }

        /// <summary>
        /// The ID of the build definition.
        /// </summary>
        public static string DefinitionId
        {
            get { return global::System.Environment.GetEnvironmentVariable("SYSTEM_DEFINITIONID"); }
        }

        /// <summary>
        /// If the pull request is from a fork of the repository, this variable is set to True. Otherwise, it is set to False.
        /// </summary>
        public static bool PullRequestIsFork
        {
            get { return global::System.Environment.GetEnvironmentVariable("SYSTEM_PULLREQUEST_ISFORK") == "True"; }
        }

        /// <summary>
        /// The ID of the pull request that caused this build. For example: 17. (This variable is initialized only if the build ran because of a Git PR affected by a branch policy.)
        /// </summary>
        public static string PullRequestId
        {
            get { return global::System.Environment.GetEnvironmentVariable("SYSTEM_PULLREQUEST_PULLREQUESTID"); }
        }

        /// <summary>
        /// The number of the pull request that caused this build. This variable is populated for pull requests from GitHub which have a different pull request ID and pull request number.
        /// </summary>
        public static string PullRequestNumber
        {
            get { return global::System.Environment.GetEnvironmentVariable("SYSTEM_PULLREQUEST_PULLREQUESTNUMBER"); }
        }

        /// <summary>
        /// The branch that is being reviewed in a pull request. For example: refs/heads/users/raisa/new-feature. (This variable is initialized only if the build ran because of a Git PR affected by a branch policy.)
        /// </summary>
        public static string PullRequestSourceBranch
        {
            get { return global::System.Environment.GetEnvironmentVariable("SYSTEM_PULLREQUEST_SOURCEBRANCH"); }
        }

        /// <summary>
        /// VSTS Only. The URL to the repo that contains the pull request. For example: https://ouraccount.visualstudio.com/_git/OurProject. (This variable is initialized only if the build ran because of a VSTS Git PR affected by a branch policy. It is not initialized for GitHub PRs.)
        /// </summary>
        public static string PullRequestSourceRepositoryUri
        {
            get { return global::System.Environment.GetEnvironmentVariable("SYSTEM_PULLREQUEST_SOURCEREPOSITORYURI"); }
        }

        /// <summary>
        /// The branch that is the target of a pull request. For example: refs/heads/master. (This variable is initialized only if the build ran because of a Git PR affected by a branch policy.)
        /// </summary>
        public static string PullRequestTargetBranch
        {
            get { return global::System.Environment.GetEnvironmentVariable("SYSTEM_PULLREQUEST_TARGETBRANCH"); }
        }

        /// <summary>
        /// The URI of the team foundation collection. For example: https://fabrikamfiber.visualstudio.com/.
        /// </summary>
        public static string TeamFoundationCollectionUri
        {
            get { return global::System.Environment.GetEnvironmentVariable("SYSTEM_TEAMFOUNDATIONCOLLECTIONURI"); }
        }

        /// <summary>
        /// The name of the team project that contains this build.
        /// </summary>
        public static string TeamProject
        {
            get { return global::System.Environment.GetEnvironmentVariable("SYSTEM_TEAMPROJECT"); }
        }

        /// <summary>
        /// The ID of the team project that this build belongs to.
        /// </summary>
        public static string TeamProjectId
        {
            get { return global::System.Environment.GetEnvironmentVariable("SYSTEM_TEAMPROJECTID"); }
        }
    }
}