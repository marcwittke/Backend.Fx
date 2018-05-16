namespace Backend.Fx.AspNetCore.Integration
{
    using System;
    using Logging;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class BackendFxProgram<TStartup> where TStartup : class
    {
        private static Logging.ILogger logger = new DebugLogger(nameof(BackendFxProgram<TStartup>));

        public void Main(string[] args)
        {
            IWebHost webHost;
            try
            {
                webHost = BuildWebHost(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            try
            {
                using (logger.InfoDuration("Starting web host", "Web host stopped"))
                {
                    webHost.Run();
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Web host died unexpectedly");
                LogManager.Shutdown();
                throw;
            }
        }

        public IWebHost BuildWebHost(string[] args)
        {
            try
            {
                logger = LogManager.Create<BackendFxProgram<TStartup>>();
                logger.Debug("Logging initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Logging cannot be initialized: " + ex);
                throw;
            }

            try
            {
                IWebHost host;
                using (logger.InfoDuration("Building web host"))
                {
                    host = WebHost.CreateDefaultBuilder(args)
                                  .ConfigureLogging(builder =>
                                                    {
                                                        builder.ClearProviders();
                                                        builder.AddProvider(new BackendFxLoggerProvider());
                                                        builder.AddDebug();
                                                    })
                                  .UseStartup<TStartup>()
                                  .Build();

                    host.Services.GetRequiredService<IApplicationLifetime>().ApplicationStopped.Register(LogManager.Shutdown);
                }

                return host;
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Web host could not be built.");
                LogManager.Shutdown();
                throw;
            }
        }
    }
}