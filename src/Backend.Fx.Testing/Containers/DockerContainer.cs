namespace Backend.Fx.Testing.Containers
{
    using System;
    using System.Threading.Tasks;
    using Docker.DotNet;
    using Docker.DotNet.Models;
    using Fx.Logging;
    using JetBrains.Annotations;
    using Polly;

    /// <summary>
    /// An abstraction over a container running on local docker. Communication is done using the Docker API
    /// </summary>
    public abstract class DockerContainer : IDisposable
    {
        private static readonly ILogger Logger = LogManager.Create<DockerContainer>();

        protected DockerContainer([NotNull] string baseImage, string name, string dockerApiUrl, string containerId = null)
        {
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
        public async Task CreateAndStart()
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

        public async Task Kill()
        {
            if (ContainerId == null)
            {
                throw new InvalidOperationException($"Container has not been created.");
            }

            Logger.Info($"Killing container {ContainerId}");
            await Client.Containers.KillContainerAsync(ContainerId, new ContainerKillParameters());
            Logger.Info($"Container {ContainerId} was killed successfully");
        }

        /// <summary>
        /// Kills and removes the container
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (ContainerId != null)
                {
                    try
                    {
                        Kill().Wait();
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, $"Failed to kill container {ContainerId}");
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}