using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.Environment.Persistence
{
    public class BackendFxDbApplication : IBackendFxApplication
    {
        private static readonly ILogger Logger = LogManager.Create<BackendFxDbApplication>();
        
        private readonly IDatabaseBootstrapper _databaseBootstrapper;
        private readonly IDatabaseAvailabilityAwaiter _databaseAvailabilityAwaiter;
        private readonly IBackendFxApplication _backendFxApplication;

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
            Logger.Trace("Disposing...");
            _databaseBootstrapper.Dispose();
            _backendFxApplication.Dispose();
        }

        public IBackendFxApplicationAsyncInvoker AsyncInvoker => _backendFxApplication.AsyncInvoker;

        public ICompositionRoot CompositionRoot => _backendFxApplication.CompositionRoot;

        public IBackendFxApplicationInvoker Invoker => _backendFxApplication.Invoker;

        public IMessageBus MessageBus => _backendFxApplication.MessageBus;

        public bool WaitForBoot(int timeoutMilliSeconds = Int32.MaxValue, CancellationToken cancellationToken = default)
        {
            Logger.Trace("Waiting for boot...");
            return _backendFxApplication.WaitForBoot(timeoutMilliSeconds, cancellationToken);
        }

        public async Task Boot(CancellationToken cancellationToken = default)
        {
            Logger.Trace("Booting...");
            await _databaseAvailabilityAwaiter.WaitForDatabase(cancellationToken);
            _databaseBootstrapper.EnsureDatabaseExistence();
            await _backendFxApplication.Boot(cancellationToken);
        }
    }
}