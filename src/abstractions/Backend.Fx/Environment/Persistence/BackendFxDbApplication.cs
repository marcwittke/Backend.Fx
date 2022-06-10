using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Environment.Persistence
{
    public class BackendFxDbApplication : BackendFxApplicationDecorator
    {
        private static readonly ILogger Logger = Log.Create<BackendFxDbApplication>();

        private readonly IDatabaseAvailabilityAwaiter _databaseAvailabilityAwaiter;
        private readonly IDatabaseBootstrapper _databaseBootstrapper;

        public BackendFxDbApplication(IDatabaseBootstrapper databaseBootstrapper,
            IDatabaseAvailabilityAwaiter databaseAvailabilityAwaiter,
            IBackendFxApplication application) : base(application)
        {
            _databaseBootstrapper = databaseBootstrapper;
            _databaseAvailabilityAwaiter = databaseAvailabilityAwaiter;
        }


        public override async Task BootAsync(CancellationToken cancellationToken = default)
        {
            Logger.LogTrace("Booting...");
            await _databaseAvailabilityAwaiter.WaitForDatabase(cancellationToken).ConfigureAwait(false);
            _databaseBootstrapper.EnsureDatabaseExistence();
            await base.BootAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}