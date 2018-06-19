namespace Backend.Fx.Tests.Containers
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using RandomData;
    using Testing.BuildEnv;
    using Testing.Containers;
    using Xunit;

    public class TestContainer : MssqlDockerContainer
    {
        public TestContainer(string dockerApiUrl, string name)
            : base(dockerApiUrl, TestRandom.NextPassword(), name)
        { }

        public override IDbConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }

    public class TheMssqlDockerContainer : IAsyncLifetime
    {
        private string dockerApiUri;
        private string containerName;
        private TestContainer container;

        [Fact]
        public async Task CanBeUsed()
        {
            containerName = CreateContainerName("TheMssqlDockerContainer_CanBeUsed");
            
            container = new TestContainer(dockerApiUri, containerName);
            await container.InitializeAsync();
            await container.CreateAndStartAsync();
            Assert.False(container.HealthCheck());
            Assert.True(container.WaitUntilIsHealthy());
        }

        [Fact]
        public async Task CanRestore()
        {
            containerName = CreateContainerName("TheMssqlDockerContainer_CanRestore");
            await DockerUtilities.EnsureKilledAndRemoved(dockerApiUri, containerName);
            container = new TestContainer(dockerApiUri, containerName);

            await container.CreateAndStartAsync();
            Assert.False(container.HealthCheck());
            Assert.True(container.WaitUntilIsHealthy());

            await container.Restore("Backup.bak", "RestoredDb");
        }

        private static string CreateContainerName(string name)
        {
            if (Build.IsTfBuild)
            {
                return $"{name}_agent{Agent.Id}";
            }

            return name;
        }

        public async Task InitializeAsync()
        {
            dockerApiUri = await DockerUtilities.DetectDockerClientApi();
            if (Build.IsTfBuild)
            {
                await DockerUtilities.KillAllOlderThan(dockerApiUri, TimeSpan.FromMinutes(30));
            }
        }

        public async Task DisposeAsync()
        {
            await container.EnsureKilledAsync();
        }
    }
}
