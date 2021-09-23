using Backend.Fx.AspNetCore.Tests.SampleApp;
using Backend.Fx.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Backend.Fx.AspNetCore.Tests
{
    public class SampleAppWebApplicationFactory : WebApplicationFactory<Startup>
    {
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            var webHostBuilder = new WebHostBuilder();
            return webHostBuilder;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .ConfigureLogging(
                    loggingBuilder =>
                    {
                        loggingBuilder.ClearProviders();
                        loggingBuilder.AddProvider(new BackendFxLoggerProvider());
                        loggingBuilder.AddDebug();
                    })
                .CaptureStartupErrors(true)
                .UseSolutionRelativeContentRoot("")
                .UseSetting("detailedErrors", true.ToString())
                .UseEnvironment("Development")
                .UseStartup<Startup>();
        }

        protected override TestServer CreateServer(IWebHostBuilder builder)
        {
            var server = base.CreateServer(builder);

            var tenantService = server.Services.GetRequiredService<SampleApplicationHostedService>().TenantService;
            for (var i = 0; i < 100; i++)
            {
                var x = tenantService.CreateTenant($"t{i:000}", $"Tenant {i:000}", false);
                Assert.True(x.Value > 0);
            }


            return server;
        }
    }
}
