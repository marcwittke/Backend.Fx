using Backend.Fx.Environment.Persistence;
using Backend.Fx.Logging;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence.Bootstrapping
{
    public abstract class EfDatabaseBootstrapper<TDbContext> : IDatabaseBootstrapper where TDbContext : DbContext
    {
        private readonly TDbContext _dbContext;
        private readonly IDatabaseBootstrapperInstanceProvider _instanceProvider;
        private static readonly ILogger Logger = LogManager.Create<EfDatabaseBootstrapper<TDbContext>>();

        protected EfDatabaseBootstrapper(TDbContext dbContext,IDatabaseBootstrapperInstanceProvider instanceProvider)
        {
            _dbContext = dbContext;
            _instanceProvider = instanceProvider;
        }

        public void EnsureDatabaseExistence()
        {
            Logger.Info("Ensuring database existence");
            ExecuteCreationStrategy(_dbContext);
            EnsureSearchIndexExistence();
            EnsureSequenceExistence();
        }

        private void EnsureSearchIndexExistence()
        {
            Logger.Info("Ensuring existence of full text search indizes");
            foreach (var fullTextSearchIndex in _instanceProvider.GetAllSearchIndizes())
            {
                fullTextSearchIndex.EnsureIndex(_dbContext);
            }
        }

        private void EnsureSequenceExistence()
        {
            Logger.Info("Ensuring existence of sequences");
            foreach (var sequence in _instanceProvider.GetAllSequences())
            {
                sequence.EnsureSequence(_dbContext.Database.GetDbConnection());
            }
        }

        protected abstract void ExecuteCreationStrategy(TDbContext dbContext);
    }
}
