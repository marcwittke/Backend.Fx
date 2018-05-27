namespace Backend.Fx.Tests.Containers
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Environment.VisualStudioOnline;
    using Fx.Extensions;
    using RandomData;
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

    public class TheMssqlDockerContainer
    {
        private readonly string dockerApiUri;

        public TheMssqlDockerContainer()
        {
            dockerApiUri = AsyncHelper.RunSync(() => DockerUtilities.DetectDockerClientApi());
            if (Build.IsTfBuild)
            {
                AsyncHelper.RunSync(() => DockerUtilities.KillAllOlderThan(dockerApiUri, TimeSpan.FromMinutes(30)));
            }
        }

        [Fact]
        public async Task CanBeUsed()
        {
            var containerName = CreateContainerName("TheMssqlDockerContainer_CanBeUsed");
            await DockerUtilities.EnsureKilledAndRemoved(dockerApiUri, containerName);
            using (TestContainer container = new TestContainer(dockerApiUri, containerName))
            {
                await container.CreateAndStart();
                Assert.False(container.HealthCheck());
                Assert.True(container.WaitUntilIsHealthy());
            }
        }

        [Fact]
        public async Task CanRestore()
        {
            var containerName = CreateContainerName("TheMssqlDockerContainer_CanRestore");
            await DockerUtilities.EnsureKilledAndRemoved(dockerApiUri, containerName);
            using (TestContainer container = new TestContainer(dockerApiUri, containerName))
            {
                await container.CreateAndStart();
                Assert.False(container.HealthCheck());
                Assert.True(container.WaitUntilIsHealthy());

                await container.Restore("Backup.bak", "RestoredDb");
            }
        }

        private static string CreateContainerName(string name)
        {
            if (Build.IsTfBuild)
            {
                return $"{name}_agent{Agent.Id}";
            }

            return name;
        }
    }
}
