namespace Backend.Fx.Testing.OnContainers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Docker.DotNet;
    using Docker.DotNet.Models;
    using Fx.Logging;

    public class DockerDiscovery
    {
        private static readonly ILogger Logger = LogManager.Create<DockerDiscovery>();

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
                var client = new DockerClientConfiguration(new Uri(uriToDetect)).CreateClient();
                VersionResponse version = await client.System.GetVersionAsync();
                Logger.Info($"Docker API version {version.APIVersion} detected at {uriToDetect}");
                return uriToDetect;
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, $"Check for Docker API at {uriToDetect} failed");
            }

            return null;
        }
    }
}
