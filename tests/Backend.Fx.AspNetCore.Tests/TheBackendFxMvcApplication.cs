using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Backend.Fx.AspNetCore.Tests.SampleApp.Domain;
using Backend.Fx.Tests;
using Backend.Fx.TestUtil;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.AspNetCore.Tests
{
    public class TheBackendFxMvcApplication : TestWithLogging
    {
        
        private readonly SampleAppWebApplicationFactory _factory;
        
        public TheBackendFxMvcApplication(ITestOutputHelper output) : base(output)
        {
            _factory = new SampleAppWebApplicationFactory(Logger);
        }
        
        [Fact]
        public async Task CanBeCalledWithCorrectArguments()
        {
            using (var client = _factory.CreateClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {JwtService.IssueJwt("testUser")}");
                var result = await client.PostAsync("/api/calculations/addition/4/8?tenantId=1234", new StringContent(""));
                var stringResult = await result.Content.ReadAsStringAsync();

                Assert.True(result.IsSuccessStatusCode);

                var calculationResult = JsonConvert.DeserializeObject<ICalculationService.CalculationResult>(stringResult);
                Assert.Equal(12d, calculationResult.Result);
            }
        }
        
        [Fact]
        public async Task HandlesClientErrors()
        {
            using (var client = _factory.CreateClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {JwtService.IssueJwt("testUser")}");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                var result = await client.PostAsync("/api/calculations/division/4/0?tenantId=1234", new StringContent(""));
                var stringResult = await result.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
                Assert.Contains("Division by zero", stringResult);
            }
        }
        
        
        [Fact]
        public async Task MaintainsTheCurrentTenantId()
        {
            using (var client = _factory.CreateClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {JwtService.IssueJwt("testUser")}");
                var result = await client.PostAsync("/api/calculations/addition/4/8?tenantId=1234", new StringContent(""));
                var stringResult = await result.Content.ReadAsStringAsync();

                Assert.True(result.IsSuccessStatusCode);

                var calculationResult = JsonConvert.DeserializeObject<ICalculationService.CalculationResult>(stringResult);
                Assert.Equal(1234, calculationResult.TenantId);
            }
        }
        
        [Fact]
        public async Task MaintainsTheCurrentIdentity()
        {
            using (var client = _factory.CreateClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {JwtService.IssueJwt("testUser")}");
                var result = await client.PostAsync("/api/calculations/addition/4/8?tenantId=1234", new StringContent(""));
                var stringResult = await result.Content.ReadAsStringAsync();

                Assert.True(result.IsSuccessStatusCode);

                var calculationResult = JsonConvert.DeserializeObject<ICalculationService.CalculationResult>(stringResult);
                Assert.Equal("testUser", calculationResult.Executor);
            }
        }
    }
}