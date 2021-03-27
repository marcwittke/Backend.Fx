using System.Net.Http.Json;
using System.Threading.Tasks;
using Backend.Fx.AspNetCore.MultiTenancy;
using Backend.Fx.RandomData;
using Newtonsoft.Json;
using Xunit;

namespace Backend.Fx.AspNetCore.Tests
{
    public class TheMultiTenantApplication
    {
        private readonly SampleAppWebApplicationFactory _factory = new();

        [Fact]
        public async Task ProvidesTenant()
        {
            using (var client = _factory.CreateClient())
            {
                var response = await client.GetAsync("/api/tenants/21");
                var responseContent = await response.Content.ReadAsStringAsync();
                Assert.True(response.IsSuccessStatusCode, $"{(int)response.StatusCode}: {response.StatusCode.ToString()}");
                var tenant = JsonConvert.DeserializeAnonymousType(responseContent, new {Id = 0, Name = "", Description = ""});
                Assert.NotNull(tenant);
                Assert.Equal(21, tenant.Id);
            }
        }
        
        [Fact]
        public async Task ProvidesListOfTenants()
        {
            using (var client = _factory.CreateClient())
            {
                var response = await client.GetAsync("/api/tenants");
                var responseContent = await response.Content.ReadAsStringAsync();
                Assert.True(response.IsSuccessStatusCode, $"{(int)response.StatusCode}: {response.StatusCode.ToString()}");
                var tenants = JsonConvert.DeserializeAnonymousType(responseContent, new[] {new {Id = 0, Name = "", Description = ""}});
                Assert.NotEmpty(tenants);
                Assert.True(tenants.Length >= 100);
            }
        }

        [Fact]
        public async Task CanCreateTenant()
        {
            using (var client = _factory.CreateClient())
            {
                var createTenantParams = new CreateTenantParams
                {
                    AdministratorEmail = "me@example.com",
                    AdministratorPassword = "Pa$$w0rd",
                    Culture = "fr",
                    Description = LoremIpsumGenerator.Generate(3, 5, true),
                    IsDemo = false,
                    Name = "Hello"
                };
                
                var response = await client.PostAsJsonAsync("/api/tenants", createTenantParams);
                var responseContent = await response.Content.ReadAsStringAsync();
                Assert.True(response.IsSuccessStatusCode, $"{(int)response.StatusCode}: {response.StatusCode.ToString()}");
                
                var tenant = JsonConvert.DeserializeAnonymousType(responseContent, new {Id = 0, Name = "", Description = ""});
                Assert.True(tenant.Id != 0);
                Assert.Equal(createTenantParams.Name, tenant.Name);
            }
        }
    }
}