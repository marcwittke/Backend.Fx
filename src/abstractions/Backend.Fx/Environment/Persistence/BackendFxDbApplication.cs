using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.Environment.Persistence
{
    public class BackendFxDbApplication : IBackendFxApplication
    {
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
            _databaseBootstrapper.Dispose();
            _backendFxApplication.Dispose();
        }

        public IBackendFxApplicationAsyncInvoker AsyncInvoker => _backendFxApplication.AsyncInvoker;

        public ICompositionRoot CompositionRoot => _backendFxApplication.CompositionRoot;

        public IBackendFxApplicationInvoker Invoker => _backendFxApplication.Invoker;

        public IMessageBus MessageBus => _backendFxApplication.MessageBus;

        public bool WaitForBoot(int timeoutMilliSeconds = Int32.MaxValue, CancellationToken cancellationToken = default)
        {
            return _backendFxApplication.WaitForBoot(timeoutMilliSeconds, cancellationToken);
        }

        public async Task Boot(CancellationToken cancellationToken = default)
        {
            await _databaseAvailabilityAwaiter.WaitForDatabase(cancellationToken);
            _databaseBootstrapper.EnsureDatabaseExistence();
            await _backendFxApplication.Boot(cancellationToken);
        }
    }
}