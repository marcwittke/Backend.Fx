using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.DependencyInjection;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Extensions.Persistence
{
    [PublicAPI]
    public class PersistentApplication : BackendFxApplicationExtension
    {
        private static readonly ILogger Logger = Log.Create<PersistentApplication>();

        private readonly IDatabaseAvailabilityAwaiter _databaseAvailabilityAwaiter;
        private readonly IDatabaseBootstrapper _databaseBootstrapper;

        public PersistentApplication(IModule persistenceModule, IBackendFxApplication application) 
            : this(null, null, persistenceModule, application)
        {
        }
        
        public PersistentApplication(
            IDatabaseBootstrapper databaseBootstrapper,
            IModule persistenceModule,
            IBackendFxApplication application) 
            : this(databaseBootstrapper, null, persistenceModule, application)
        {
        }

        public PersistentApplication(
            IDatabaseBootstrapper databaseBootstrapper,
            IDatabaseAvailabilityAwaiter databaseAvailabilityAwaiter,
            IModule persistenceModule,
            IBackendFxApplication application)
            : base(application)
        {
            _databaseBootstrapper = databaseBootstrapper;
            _databaseAvailabilityAwaiter = databaseAvailabilityAwaiter;
            application.CompositionRoot.RegisterModules(persistenceModule);
        }


        public override async Task BootAsync(CancellationToken cancellationToken = default)
        {
            Logger.LogTrace("Booting...");
            if (_databaseAvailabilityAwaiter != null)
            {
                await _databaseAvailabilityAwaiter.WaitForDatabase(cancellationToken).ConfigureAwait(false);
            }

            _databaseBootstrapper?.EnsureDatabaseExistence();
            await base.BootAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}