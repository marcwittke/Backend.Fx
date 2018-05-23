namespace Backend.Fx.Tests.Containers
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Environment;
    using Environment.VisualStudioOnline;
    using Fx.Extensions;
    using RandomData;
    using Testing.Containers;
    using Xunit;

    public class TestContainer : MssqlDockerContainer
    {
        public TestContainer(string dockerApiUrl, string name) 
                : base(dockerApiUrl, TestRandom.NextPassword(), name + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss"))
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
            dockerApiUri = AsyncHelper.RunSync(()=> DockerUtilities.DetectDockerClientApi());
            if (Build.IsTfBuild) 
            {
                AsyncHelper.RunSync(()=> DockerUtilities.KillAllOlderThan(dockerApiUri, TimeSpan.FromMinutes(30)));
            }
        }

        [Fact]
        public async Task CanBeUsed()
        {
            using (TestContainer container = new TestContainer(dockerApiUri, "TheMssqlDockerContainer_CanBeUsed"))
            {
                await container.CreateAndStart();
                Assert.False(container.HealthCheck());
                Assert.True(container.WaitUntilIsHealthy());
            }
        }

        [Fact]
        public async Task CanRestore()
        {
            using (TestContainer container = new TestContainer(dockerApiUri, "TheMssqlDockerContainer_CanRestore"))
            {
                await container.CreateAndStart();
                Assert.False(container.HealthCheck());
                Assert.True(container.WaitUntilIsHealthy());

                await container.Restore("Backup.bak", "RestoredDb");
            }
        }
    }
}
