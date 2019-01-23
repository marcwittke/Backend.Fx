using System;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Logging;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence.Bootstrapping
{
    public abstract class EfDatabaseBootstrapper<TDbContext> : IDatabaseBootstrapper where TDbContext : DbContext
    {
        private readonly Func<TDbContext> _dbContextFactory;
        private readonly IDatabaseBootstrapperInstanceProvider _instanceProvider;
        private static readonly ILogger Logger = LogManager.Create<EfDatabaseBootstrapper<TDbContext>>();

        protected EfDatabaseBootstrapper(Func<TDbContext> dbContextFactoryFactory, IDatabaseBootstrapperInstanceProvider instanceProvider)
        {
            _dbContextFactory = dbContextFactoryFactory;
            _instanceProvider = instanceProvider;
        }

        public void EnsureDatabaseExistence()
        {
            Logger.Info("Ensuring database existence");
            using (var dbContext = _dbContextFactory())
            {
                ExecuteCreationStrategy(dbContext);
                EnsureSearchIndexExistence(dbContext);
                EnsureSequenceExistence(dbContext);
            }
        }

        private void EnsureSearchIndexExistence(TDbContext dbContext)
        {
            Logger.Info("Ensuring existence of full text search indizes");
            foreach (var fullTextSearchIndex in _instanceProvider.GetAllSearchIndizes())
            {
                fullTextSearchIndex.EnsureIndex(dbContext);
            }
        }

        private void EnsureSequenceExistence(TDbContext dbContext)
        {
            Logger.Info("Ensuring existence of sequences");
            foreach (var sequence in _instanceProvider.GetAllSequences())
            {
                sequence.EnsureSequence(dbContext.Database.GetDbConnection());
            }
        }

        protected abstract void ExecuteCreationStrategy(TDbContext dbContext);
    }
}
