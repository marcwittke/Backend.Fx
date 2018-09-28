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
        private string _dockerApiUri;
        private string _containerName;
        private TestContainer _container;

        [Fact]
        public async Task CanBeUsed()
        {
            _containerName = CreateContainerName("TheMssqlDockerContainer_CanBeUsed");
            
            _container = new TestContainer(_dockerApiUri, _containerName);
            await _container.InitializeAsync();
            await _container.CreateAndStartAsync();
            Assert.False(_container.HealthCheck());
            Assert.True(_container.WaitUntilIsHealthy());
        }

        [Fact]
        public async Task CanRestore()
        {
            _containerName = CreateContainerName("TheMssqlDockerContainer_CanRestore");
            await DockerUtilities.EnsureKilledAndRemoved(_dockerApiUri, _containerName);
            _container = new TestContainer(_dockerApiUri, _containerName);

            await _container.CreateAndStartAsync();
            Assert.False(_container.HealthCheck());
            Assert.True(_container.WaitUntilIsHealthy());

            await _container.Restore("Backup.bak", "RestoredDb");
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
            _dockerApiUri = await DockerUtilities.DetectDockerClientApi();
            if (Build.IsTfBuild)
            {
                await DockerUtilities.KillAllOlderThan(_dockerApiUri, TimeSpan.FromMinutes(30));
            }
        }

        public async Task DisposeAsync()
        {
            await _container.EnsureKilledAsync();
        }
    }
}
