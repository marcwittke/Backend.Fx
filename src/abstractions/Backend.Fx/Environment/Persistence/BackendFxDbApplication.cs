using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Environment.Persistence
{
    public class BackendFxDbApplication : IBackendFxApplication
    {
        private static readonly ILogger Logger = Log.Create<BackendFxDbApplication>();
        
        private readonly IDatabaseAvailabilityAwaiter _databaseAvailabilityAwaiter;
        private readonly IBackendFxApplication _backendFxApplication;
        private readonly IDatabaseBootstrapper _databaseBootstrapper;

        public BackendFxDbApplication(IDatabaseBootstrapper databaseBootstrapper,
                                      IDatabaseAvailabilityAwaiter databaseAvailabilityAwaiter,
                                      IBackendFxApplication backendFxApplication)
        {
            _databaseBootstrapper = databaseBootstrapper;
            _databaseAvailabilityAwaiter = databaseAvailabilityAwaiter;
            _backendFxApplication = backendFxApplication;
        }

        public void Dispose()
        {
            Logger.LogTrace("Disposing...");
            _backendFxApplication.Dispose();
        }

        public IBackendFxApplicationAsyncInvoker AsyncInvoker => _backendFxApplication.AsyncInvoker;

        public ICompositionRoot CompositionRoot => _backendFxApplication.CompositionRoot;

        public IBackendFxApplicationInvoker Invoker => _backendFxApplication.Invoker;

        public IMessageBus MessageBus => _backendFxApplication.MessageBus;

        public bool WaitForBoot(int timeoutMilliSeconds = Int32.MaxValue, CancellationToken cancellationToken = default)
        {
            Logger.LogTrace("Waiting for boot...");
            return _backendFxApplication.WaitForBoot(timeoutMilliSeconds, cancellationToken);
        }

        public Task Boot(CancellationToken cancellationToken = default) => BootAsync(cancellationToken);
        public async Task BootAsync(CancellationToken cancellationToken = default)
        {
            Logger.LogTrace("Booting...");
            await _databaseAvailabilityAwaiter.WaitForDatabase(cancellationToken);
            _databaseBootstrapper.EnsureDatabaseExistence();
            await _backendFxApplication.BootAsync(cancellationToken);
        }
    }
}