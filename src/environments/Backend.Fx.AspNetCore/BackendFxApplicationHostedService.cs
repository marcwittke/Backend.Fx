using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            using (Logger.InfoDuration("Application starting..."))
            {
                try
                {
                    await Application.BootAsync(ct);
                }
                catch (Exception ex)
                {
                    throw Logger.Fatal(ex, "Application could not be started");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            using (Logger.InfoDuration("Application stopping..."))
            {
                Application.Dispose();
                return Task.CompletedTask;
            }
        }
    }
}