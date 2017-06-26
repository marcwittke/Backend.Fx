
namespace DemoBlog.Mvc
{
    using System.IO;
    using Backend.Fx.Logging;
    using Microsoft.AspNetCore.Hosting;
    using Backend.Fx.NLogLogging;
    using Infrastructure;
    using Microsoft.Extensions.DependencyInjection;
    using NLog.Config;

    public class Program
    {
        //this class was just extended with NLog initialization and some detailed logging
        public static void Main(string[] args)
        {
            var contentRoot = Directory.GetCurrentDirectory();

            LogManager.Initialize(new NLogLoggerFactory());
            ConfigurationItemFactory.Default.Targets.RegisterDefinition("ApplicationInsightsTarget", typeof(ApplicationInsightsTarget));
            NLog.LogManager.Configuration = new XmlLoggingConfiguration(Path.Combine(contentRoot, "nlog.config"));

            var logger = LogManager.Create<Program>();

            IWebHost host;
            using (logger.InfoDuration("Building web host"))
            {
                host = new WebHostBuilder()
                        .UseLoggerFactory(new FrameworkToBackendFxLoggerFactory())
                    .CaptureStartupErrors(true)
                    .UseSetting("detailedErrors", "true")
                    .UseKestrel()
                    .UseContentRoot(contentRoot)
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .UseApplicationInsights()
                    .Build();

                host.Services.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>().AddProvider(new BackendFxLoggerProvider());
            }

            using (logger.InfoDuration(
                $"Running {host.Services.GetRequiredService<IHostingEnvironment>().EnvironmentName} web host with content root {contentRoot}",
                "Web host was shut down"))
            {
                host.Run();
            }
        }
    }
}
