using Backend.Fx.AspNetCore.Tests.SampleApp;
using Backend.Fx.Environment.MultiTenancy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Xunit;
using ILogger = Serilog.ILogger;

namespace Backend.Fx.AspNetCore.Tests
{
    public class SampleAppWebApplicationFactory : WebApplicationFactory<Startup>
    {
        private readonly ILogger _logger;

        public SampleAppWebApplicationFactory(ILogger logger)
        {
            _logger = logger;
        }
        
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            var webHostBuilder = new WebHostBuilder();
            return webHostBuilder;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .UseSerilog(_logger)
                .UseSolutionRelativeContentRoot("")
                .UseSetting("detailedErrors", true.ToString())
                .UseEnvironment("Development")
                .UseStartup<Startup>();
        }

        protected override TestServer CreateServer(IWebHostBuilder builder)
        {
            TestServer server = base.CreateServer(builder);

            ITenantService tenantService = server.Services.GetRequiredService<ITenantService>();
            for (int i = 0; i < 100; i++)
            {
                var x = tenantService.CreateTenant($"t{i:000}", $"Tenant {i:000}", false);
                Assert.True(x.Value > 0);
            }
            
            
            return server;
        }
    }
}