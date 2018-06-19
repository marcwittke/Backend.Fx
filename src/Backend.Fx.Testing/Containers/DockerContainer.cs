namespace Backend.Fx.Testing.Containers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Docker.DotNet;
    using Docker.DotNet.Models;
    using Fx.Logging;
    using JetBrains.Annotations;
    using Polly;
    using Xunit;

    /// <summary>
    /// An abstraction over a container running on local docker. Communication is done using the Docker API
    /// </summary>
    public abstract class DockerContainer : IAsyncLifetime
    {
        private readonly string dockerApiUrl;
        private static readonly ILogger Logger = LogManager.Create<DockerContainer>();

        protected DockerContainer([NotNull] string baseImage, string name, string dockerApiUrl, string containerId = null)
        {
            this.dockerApiUrl = dockerApiUrl;
            BaseImage = baseImage ?? throw new ArgumentNullException(nameof(baseImage));
            Name = name;
            ContainerId = containerId;
            Client = new DockerClientConfiguration(new Uri(dockerApiUrl)).CreateClient();
        }

        public string BaseImage { get; }
        public string Name { get; private set; }

        public string ContainerId { get; private set; }

        protected abstract CreateContainerParameters CreateParameters { get; }

        /// <summary>
        /// Return true from your implementation, when the container is running and can be used by clients
        /// </summary>
        /// <returns></returns>
        public abstract bool HealthCheck();

        public bool WaitUntilIsHealthy(int retries = 10)
        {
            return Policy
                   .HandleResult<bool>(r => !r)
                   .WaitAndRetry(retries,
                                 retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                 (result, span) =>
                                 {
                                     Logger.Info(result.Result
                                                         ? $"Container {ContainerId} is healthy"
                                                         : $"Container {ContainerId} not yet healthy");
                                 })
                   .Execute(HealthCheck);
        }

        protected DockerClient Client { get; }

        /// <summary>
        /// Creates a container from the base image and starts it
        /// </summary>
        /// <returns></returns>
        public async Task CreateAndStartAsync()
        {
            if (ContainerId != null)
            {
                throw new InvalidOperationException($"Container {ContainerId} has been created before.");
            }

            Logger.Info($"Creating container from base image {BaseImage}");
            CreateContainerResponse response = await Client.Containers.CreateContainerAsync(CreateParameters);
            if (Name == null)
            {
                var inspect = await Client.Containers.InspectContainerAsync(ContainerId);
                Name = inspect.Name;
            }
            ContainerId = response.ID;
            Logger.Info($"Container {ContainerId} successfully created");

            Logger.Info($"Starting container {ContainerId}");
            bool isStarted = await Client.Containers.StartContainerAsync(ContainerId, new ContainerStartParameters());
            if (!isStarted)
            {
                throw new Exception($"Starting container {ContainerId} failed");
            }
            Logger.Info($"Container {ContainerId} was started successfully");
        }

        public async Task EnsureKilledAsync()
        {

            var containersListParameters = new ContainersListParameters
            {
                All = true,
                Filters = new Dictionary<string, IDictionary<string, bool>> {
                            {
                                    "id", new Dictionary<string, bool> {
                                            {ContainerId, true},
                                    }
                            }
                    },
            };

            var container = (await Client.Containers.ListContainersAsync(containersListParameters)).FirstOrDefault();

            if (container?.State == "running")
            {
                Logger.Info($"Killing container {container.ID}");
                await Client.Containers.KillContainerAsync(container.ID, new ContainerKillParameters());
                Logger.Info($"Container {container.ID} killed");
            }

        }

        public async Task KillAsync()
        {
            if (ContainerId == null)
            {
                throw new InvalidOperationException($"Container has not been created.");
            }

            Logger.Info($"Killing container {ContainerId}");
            await Client.Containers.KillContainerAsync(ContainerId, new ContainerKillParameters());
            Logger.Info($"Container {ContainerId} was killed successfully");
        }

        public virtual async Task InitializeAsync()
        {
            await DockerUtilities.EnsureKilledAndRemoved(dockerApiUrl, Name);
        }

        public virtual async Task DisposeAsync()
        {
            if (ContainerId != null)
            {
                try
                {
                    await EnsureKilledAsync();
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, $"Failed to kill container {ContainerId}");
                }
            }
        }
    }
}