using Backend.Fx.AspNetCore.Tests.SampleApp;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Xunit;
using ILogger = Serilog.ILogger;

namespace Backend.Fx.AspNetCore.Tests
{
    public class SampleAppWebApplicationFactory : WebApplicationFactory<Startup>
    {
        private static readonly ILogger Logger = CreateLogger();

        public SampleAppWebApplicationFactory()
        {
            LogManager.Init(new SerilogLoggerFactory(Logger));
        }
        
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            var webHostBuilder = new WebHostBuilder();
            return webHostBuilder;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .UseSerilog(Logger)
                .UseSolutionRelativeContentRoot("")
                .UseSetting("detailedErrors", true.ToString())
                .UseEnvironment("Development")
                .UseStartup<Startup>();
        }

        protected override TestServer CreateServer(IWebHostBuilder builder)
        {
            TestServer server = base.CreateServer(builder);

            ITenantService tenantService = server.Services.GetRequiredService<SampleApplicationHostedService>().TenantService;
            for (int i = 0; i < 100; i++)
            {
                var x = tenantService.CreateTenant($"t{i:000}", $"Tenant {i:000}", false);
                Assert.True(x.Value > 0);
            }
            
            
            return server;
        }
        
        private static ILogger CreateLogger()
        {
            ILogger serilogLogger = new LoggerConfiguration()
                                            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                                            .Enrich.FromLogContext()
                                            .WriteTo.Console()
                                            .CreateLogger();

            return serilogLogger;
        }
    }
}