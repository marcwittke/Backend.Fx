using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Backend.Fx.Features.Persistence
{
    [PublicAPI]
    public sealed class PersistenceFeature : Feature, IBootableFeature, IMultiTenancyFeature
    {
        private readonly PersistenceModule _persistenceModule;
        private readonly IDatabaseAvailabilityAwaiter _databaseAvailabilityAwaiter;
        private readonly IDatabaseBootstrapper _databaseBootstrapper;


        public PersistenceFeature(
            PersistenceModule persistenceModule,
            IDatabaseAvailabilityAwaiter databaseAvailabilityAwaiter = null,
            IDatabaseBootstrapper databaseBootstrapper = null)
        {
            _persistenceModule = persistenceModule;
            _databaseAvailabilityAwaiter = databaseAvailabilityAwaiter;
            _databaseBootstrapper = databaseBootstrapper;
        }

        public override void Enable(IBackendFxApplication application)
        {
            application.CompositionRoot.RegisterModules(_persistenceModule);
        }

        public void EnableMultiTenancyServices(IBackendFxApplication application)
        {
            if (_persistenceModule.MultiTenancyModule == null)
            {
                throw new InvalidOperationException($"No multi tenancy module provided by {_persistenceModule.GetType().Name}");
            }
            
            application.CompositionRoot.RegisterModules(_persistenceModule.MultiTenancyModule);
        }

        public async Task BootAsync(IBackendFxApplication application, CancellationToken cancellationToken = default)
        {
            if (_databaseAvailabilityAwaiter != null)
            {
                await _databaseAvailabilityAwaiter.WaitForDatabase(cancellationToken).ConfigureAwait(false);
            }
            
            if (_databaseBootstrapper != null)
            {
                await _databaseBootstrapper.EnsureDatabaseExistenceAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}