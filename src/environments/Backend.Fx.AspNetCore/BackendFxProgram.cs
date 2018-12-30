namespace Backend.Fx.AspNetCore
{
    using System;
    using Logging;
    using Backend.Fx.NetCore.Logging;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Logging;

    public class BackendFxProgram<TStartup> where TStartup : class
    {
        private static Backend.Fx.Logging.ILogger _logger = new DebugLogger(nameof(BackendFxProgram<TStartup>));

        public virtual void Main(string[] args)
        {
            IWebHost webHost;
            try
            {
                webHost = CreateWebHostBuilder(args).Build();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            try
            {
                using (_logger.InfoDuration("Starting web host", "Web host stopped"))
                {
                    webHost.Run();
                }
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "Web host died unexpectedly");
                throw;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        public IWebHostBuilder CreateWebHostBuilder(string[] args)
        {

            try
            {
                _logger = LogManager.Create<BackendFxProgram<TStartup>>();
                _logger.Debug("Logging initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Logging cannot be initialized: " + ex);
                throw;
            }

            try
            {
                using (_logger.InfoDuration("Building web host"))
                {
                    var webHostBuilder = WebHost.CreateDefaultBuilder(args)
                        .ConfigureLogging(builder =>
                        {
                            builder.ClearProviders();
                            builder.AddProvider(new BackendFxLoggerProvider());
                            builder.AddDebug();
                            builder.SetMinimumLevel(LogLevel.Debug);
                        })
                        .UseStartup<TStartup>();

                    ConfigureWebHost(webHostBuilder);

                    return webHostBuilder;
                }
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "Web host builder could not be created.");
                LogManager.Shutdown();
                throw;
            }
        }

        protected virtual void ConfigureWebHost(IWebHostBuilder builder)
        { }
    }
}