namespace Backend.Fx.Tests.Containers
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using RandomData;
    using Testing.Containers;
    using Testing.OnContainers;
    using Xunit;

    public class TestContainer : MssqlDockerContainer
    {
        public TestContainer(string dockerApiUrl) 
                : base(dockerApiUrl, TestRandom.NextPassword(), "BackendFxTests" + DateTime.UtcNow.ToString("yyyMMdd_hhMMss"))
        { }

        public override IDbConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }

    public class TheMssqlDockerContainer
    {
        [Fact]
        public async Task CanBeUsed()
        {
            string apiUrl = await DockerDiscovery.DetectDockerClientApi();

            using (TestContainer container = new TestContainer(apiUrl))
            {
                await container.CreateAndStart();
                Assert.False(container.HealthCheck());
                Assert.True(container.WaitUntilIsHealthy());
            }
        }
    }
}
