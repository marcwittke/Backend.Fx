
namespace DemoBlog.Mvc
{
    using System;
    using System.IO;
    using Backend.Fx.Logging;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Logging;
    using Backend.Fx.NLogLogging;
    using Microsoft.AspNetCore;
    using NLog.Config;

    public class Program
    {
        private static Backend.Fx.Logging.ILogger logger;

        public static void Main(string[] args)
        {
            try
            {
                LogManager.Initialize(new NLogLoggerFactory());
                ConfigurationItemFactory.Default.Targets.RegisterDefinition("ApplicationInsightsTarget", typeof(ApplicationInsightsTarget));
                NLog.LogManager.Configuration = new XmlLoggingConfiguration(Path.Combine(Directory.GetCurrentDirectory(), "nlog.config"));
                logger = LogManager.Create<Program>();
                logger.Debug("Logging initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Logging cannot be initialized: " + ex);
                Environment.Exit(-1);
            }

            try
            {
                var webHost = BuildWebHost(args);
                using (logger.InfoDuration("Starting web host", "Web host Stopped"))
                {
                    webHost.Run();
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Web host died unexpectedly");
                Environment.Exit(-2);
            }
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            IWebHost host;
            using (logger.InfoDuration("Building web host"))
            {
                host = WebHost.CreateDefaultBuilder(args)
                              .ConfigureLogging(builder => {
                                                    builder.ClearProviders();
                                                    builder.AddProvider(new BackendFxLoggerProvider());
                                                    builder.AddDebug();
                                                })
                              .UseStartup<Startup>()
                              .Build();
            }

            return host;
        }

    }
}
