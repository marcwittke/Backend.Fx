using System;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Publish);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    
    readonly string MygetApiKey = Environment.GetEnvironmentVariable("MYGET_APIKEY");
    readonly string MygetFeedUrl = Environment.GetEnvironmentVariable("MYGET_FEED_URL") ?? "https://www.myget.org/F/marcwittke/api/v3/index.json";
    readonly string NugetApiKey = Environment.GetEnvironmentVariable("NUGET_APIKEY");

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion(Framework = "netcoreapp3.1")] readonly GitVersion GitVersion;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    Target Clean => _ => _
                         .Before(Restore)
                         .Executes(() =>
                         {
                             SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                             TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                             EnsureCleanDirectory(ArtifactsDirectory);
                         });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
                           .DependsOn(Clean)
                           .DependsOn(Restore)
                           .Executes(() =>
                           {
                               DotNetBuild(s => s
                                                .SetProjectFile(Solution)
                                                .SetConfiguration(Configuration)
                                                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                                                .SetFileVersion(GitVersion.AssemblySemFileVer)
                                                .SetInformationalVersion(GitVersion.InformationalVersion)
                                                .EnableNoRestore());
                           });

    Target Test => _ => _
                        .DependsOn(Compile)
                        .Executes(() =>
                        {
                            DotNetTest(s => s
                                            .SetProjectFile(Solution)
                                            .SetConfiguration(Configuration)
                                            .EnableNoRestore());
                        });

    Target Pack => _ => _
                        .DependsOn(Test)
                        .Executes(() =>
                        {
                            foreach (var csproj in SourceDirectory.GlobFiles("**/*.csproj"))
                            {
                                DotNetPack(s => s
                                                .SetProject(csproj)
                                                .SetOutputDirectory(ArtifactsDirectory)
                                                .SetVersion(GitVersion.NuGetVersion)
                                                .SetVerbosity(DotNetVerbosity.Minimal)
                                                .SetConfiguration(Configuration));
                            }
                        });

    Target Publish => _ => _
                           .OnlyWhenDynamic(() => !IsLocalBuild && Configuration.Equals(Configuration.Release))
                           .DependsOn(Pack)
                           .Executes(() =>
                           {
                               bool pushToNuget = GitRepository.Branch == "main";

                               foreach (var nupkg in ArtifactsDirectory.GlobFiles("*.nupkg"))
                               {
                                   DotNetNuGetPush(s =>
                                   {
                                       s = s
                                           .SetTargetPath(nupkg)
                                           .EnableNoServiceEndpoint()
                                           .EnableSkipDuplicate();

                                       if (pushToNuget)
                                       {
                                           s = s.SetSource("https://api.nuget.org/v3/index.json")
                                                .SetApiKey(NugetApiKey);
                                       }
                                       else
                                       {
                                           s = s.SetSource(MygetFeedUrl)
                                                .SetApiKey(MygetApiKey);
                                       }

                                       return s;
                                   });
                               }
                           });
}