
namespace DemoBlog.Mvc
{
    using System.IO;
    using Backend.Fx.Logging;
    using Microsoft.AspNetCore.Hosting;
    using Backend.Fx.NLogLogging;
    using Microsoft.AspNetCore;
    using NLog.Config;

    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var contentRoot = Directory.GetCurrentDirectory();

            LogManager.Initialize(new NLogLoggerFactory());
            ConfigurationItemFactory.Default.Targets.RegisterDefinition("ApplicationInsightsTarget", typeof(ApplicationInsightsTarget));
            NLog.LogManager.Configuration = new XmlLoggingConfiguration(Path.Combine(contentRoot, "nlog.config"));

            var logger = LogManager.Create<Program>();
            IWebHost host;
            using (logger.InfoDuration("Building web host"))
            {
                host = WebHost.CreateDefaultBuilder(args)
                          .UseStartup<Startup>()
                          .Build();
            }

            return host;
        }
        
    }
}
