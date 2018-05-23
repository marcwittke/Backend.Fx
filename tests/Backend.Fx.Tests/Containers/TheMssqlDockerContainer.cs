namespace Backend.Fx.Tests.Containers
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Fx.Extensions;
    using RandomData;
    using Testing.Containers;
    using Xunit;

    public class TestContainer : MssqlDockerContainer
    {
        public TestContainer(string dockerApiUrl) 
                : base(dockerApiUrl, TestRandom.NextPassword(), "BackendFxTests" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss"))
        { }

        public override IDbConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }

    public class TheMssqlDockerContainer
    {
        public TheMssqlDockerContainer()
        {
            string apiUrl = AsyncHelper.RunSync(()=> DockerUtilities.DetectDockerClientApi());
            AsyncHelper.RunSync(()=> DockerUtilities.KillAllOlderThan(apiUrl, TimeSpan.FromMinutes(30)));
        }

        [Fact]
        public async Task CanBeUsed()
        {
            string apiUrl = await DockerUtilities.DetectDockerClientApi();

            using (TestContainer container = new TestContainer(apiUrl))
            {
                await container.CreateAndStart();
                Assert.False(container.HealthCheck());
                Assert.True(container.WaitUntilIsHealthy());
            }
        }
    }
}
