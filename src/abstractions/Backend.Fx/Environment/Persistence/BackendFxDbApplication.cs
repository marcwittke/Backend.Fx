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
        private readonly IBackendFxApplication _backendFxApplication;
        private readonly IDatabaseAvailabilityAwaiter _databaseAvailabilityAwaiter;

        private readonly IDatabaseBootstrapper _databaseBootstrapper;

        public BackendFxDbApplication(
            IDatabaseBootstrapper databaseBootstrapper,
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

        public bool WaitForBoot(int timeoutMilliSeconds = int.MaxValue, CancellationToken cancellationToken = default)
        {
            Logger.Trace("Waiting for boot...");
            return _backendFxApplication.WaitForBoot(timeoutMilliSeconds, cancellationToken);
        }

        public Task Boot(CancellationToken cancellationToken = default)
        {
            return BootAsync(cancellationToken);
        }

        public async Task BootAsync(CancellationToken cancellationToken = default)
        {
            Logger.Trace("Booting...");
            await _databaseAvailabilityAwaiter.WaitForDatabase(cancellationToken);
            _databaseBootstrapper.EnsureDatabaseExistence();
            await _backendFxApplication.BootAsync(cancellationToken);
        }
    }
}
