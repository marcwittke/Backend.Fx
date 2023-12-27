using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.AspNetCore
{
    [PublicAPI]
    public interface IBackendFxApplicationHostedService<out TApplication> : IHostedService
        where TApplication : IBackendFxApplication
    {
        TApplication Application { get; }
    }

    public abstract class BackendFxApplicationHostedService<TApplication> : IBackendFxApplicationHostedService<TApplication>
        where TApplication : IBackendFxApplication
    {
        private static readonly ILogger Logger = Log.Create<BackendFxApplicationHostedService<TApplication>>();

        public abstract TApplication Application { get; }

        public virtual async Task StartAsync(CancellationToken ct)
        {
            using (Logger.LogInformationDuration("Application starting..."))
            {
                try
                {
                    await Application.BootAsync(ct);
                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, "Application could not be started");
                    throw;
                }
            }
        }

        public virtual Task StopAsync(CancellationToken cancellationToken)
        {
            using (Logger.LogInformationDuration("Application stopping..."))
            {
                Application.Dispose();
                return Task.CompletedTask;
            }
        }
    }
    
    public static class BackendFxApplicationHostedServiceExtensions
    {
        public static void AddBackendFxApplication<THostedService, TApplication>(this IServiceCollection services)
            where THostedService : class, IBackendFxApplicationHostedService<TApplication>
            where TApplication : class, IBackendFxApplication
        {
            services.AddSingleton<THostedService>();
            
            // this registration ensures starting of the hosted service
            services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<THostedService>());

            // this registration makes the application instance available as singleton
            services.AddSingleton(provider => provider.GetRequiredService<THostedService>().Application);
        }
    }
}