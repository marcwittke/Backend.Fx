using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.AspNetCore
{
    public interface IBackendFxApplicationHostedService : IHostedService
    {
        IBackendFxApplication Application { get; }
    }
    
    public abstract class BackendFxApplicationHostedService : IBackendFxApplicationHostedService
    {
        private static readonly ILogger Logger = LogManager.Create<BackendFxApplicationHostedService>();

        public abstract IBackendFxApplication Application { get; }

        public async Task StartAsync(CancellationToken ct)
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

        public Task StopAsync(CancellationToken cancellationToken)
        {
            using (Logger.LogInformationDuration("Application stopping..."))
            {
                Application.Dispose();
                return Task.CompletedTask;
            }
        }
    }
}