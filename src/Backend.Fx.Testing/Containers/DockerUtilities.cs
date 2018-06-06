namespace Backend.Fx.Testing.Containers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Docker.DotNet;
    using Docker.DotNet.Models;
    using Fx.Logging;

    public class DockerUtilities
    {
        private static readonly ILogger Logger = LogManager.Create<DockerUtilities>();

        public static async Task KillAllOlderThan(string dockerApiUri, TimeSpan maxAge)
        {
            using (var client = new DockerClientConfiguration(new Uri(dockerApiUri)).CreateClient())
            {
                var list = await client.Containers.ListContainersAsync(new ContainersListParameters ());
                var tooOldContainers = list.Where(cnt => cnt.Created + maxAge < DateTime.UtcNow);
                foreach (var tooOldContainer in tooOldContainers)
                {
                    Logger.Warn($"Killing container {tooOldContainer.ID}");
                    await client.Containers.KillContainerAsync(tooOldContainer.ID, new ContainerKillParameters());
                }
            }
        }

        public static async Task<string> DetectDockerClientApi(params string[] urisToDetect)
        {
            Logger.Info("Detecting local machine's docker API url");
            urisToDetect = new[] {
                                   "npipe://./pipe/docker_engine",
                                   "http://localhost:2376",
                                   "http://localhost:2375"
                           }
                           .Concat(urisToDetect)
                           .Distinct()
                           .ToArray();

            foreach (var uriToDetect in urisToDetect)
            {
                string uri = await DetectDockerClientApi(uriToDetect);
                if (uri != null)
                {
                    return uri;
                }
            }

            Logger.Warn("No Docker API detected");
            return null;
        }

        private static async Task<string> DetectDockerClientApi(string uriToDetect)
        {
            try
            {
                Logger.Info($"Trying {uriToDetect}");
                VersionResponse version;
                using (var client = new DockerClientConfiguration(new Uri(uriToDetect)).CreateClient())
                {
                    version = await client.System.GetVersionAsync();
                }

                Logger.Info($"Docker API version {version.APIVersion} detected at {uriToDetect}");
                return uriToDetect;
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, $"Check for Docker API at {uriToDetect} failed");
            }

            return null;
        }

        public static async Task EnsureKilledAndRemoved(string dockerApiUrl, string containerName)
        {
            using (var client = new DockerClientConfiguration(new Uri(dockerApiUrl)).CreateClient())
            {
                var containersListParameters = new ContainersListParameters {
                        All = true,
                        Filters = new Dictionary<string, IDictionary<string, bool>> {
                                {
                                        "name", new Dictionary<string, bool> {
                                                {"^/"+containerName+"$", true},
                                        }
                                }
                        },
                };

                var container = (await client.Containers.ListContainersAsync(containersListParameters)).FirstOrDefault();

                if (container?.Status == "running")
                {
                    Logger.Info($"Killing container {container.ID}");
                    await client.Containers.KillContainerAsync(container.ID, new ContainerKillParameters());
                    Logger.Info($"Container {container.ID} killed");
                }

                if (container != null)
                {
                    Logger.Info($"Removing container {container.ID}");
                    await client.Containers.RemoveContainerAsync(container.ID, new ContainerRemoveParameters());
                    Logger.Info($"Container {container.ID} removed");
                }
            }
        }
    }
}
