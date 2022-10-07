using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.AspNetCore.Hosting
{
    public interface IBackendFxApplicationHostedService : IHostedService
    {
        IBackendFxApplication Application { get; }
    }

    [PublicAPI]
    public class BackendFxApplicationHostedService : IBackendFxApplicationHostedService
    {
        private static readonly ILogger Logger = Log.Create<BackendFxApplicationHostedService>();

        public BackendFxApplicationHostedService(IBackendFxApplication application)
        {
            Application = application;
        }
        
        public IBackendFxApplication Application { get; }

        public virtual async Task StartAsync(CancellationToken ct)
        {
            using (Logger.LogInformationDuration("Application starting..."))
            {
                try
                {
                    await Application.BootAsync(ct).ConfigureAwait(false);
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

}