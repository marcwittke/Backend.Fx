using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.Patterns.DependencyInjection
{
    public class BackendFxDbApplication : BackendFxApplication
    {
        /// <summary>
        /// Initializes the application's runtime instance
        /// </summary>
        /// <param name="compositionRoot">The composition root of the dependency injection framework</param>
        /// <param name="messageBus">The message bus implementation used by this application instance</param>
        /// <param name="infrastructureModule"></param>
        /// <param name="exceptionLogger">The exception logger used by this application instance</param>
        /// <param name="databaseBootstrapper">The database manager for the current application</param>
        public BackendFxDbApplication(ICompositionRoot compositionRoot,
                                         IMessageBus messageBus,
                                         IInfrastructureModule infrastructureModule,
                                         IExceptionLogger exceptionLogger,
                                         IDatabaseBootstrapper databaseBootstrapper)
            : base(compositionRoot, messageBus, infrastructureModule, exceptionLogger)
        {
            DatabaseBootstrapper = databaseBootstrapper;
        }

        /// <summary>
        /// The utility instance for database bootstrapping
        /// </summary>
        public IDatabaseBootstrapper DatabaseBootstrapper { get; }

        protected sealed override async Task OnBoot(CancellationToken cancellationToken)
        {
            await OnDatabaseBoot(cancellationToken);
            await WaitForDatabase(cancellationToken);
            DatabaseBootstrapper.EnsureDatabaseExistence();
            await OnDatabaseBooted(cancellationToken);
        }

        protected virtual async Task WaitForDatabase(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Extension point to do additional initialization before existence of database is ensured
        /// </summary>
        /// <returns></returns>
        protected virtual async Task OnDatabaseBoot(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Extension point to do additional initialization after existence of database is ensured
        /// </summary>
        /// <returns></returns>
        protected virtual async Task OnDatabaseBooted(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                DatabaseBootstrapper?.Dispose();
            }
        }
    }
}