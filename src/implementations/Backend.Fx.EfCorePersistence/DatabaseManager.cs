namespace Backend.Fx.EfCorePersistence
{
    using System.Data;
    using Environment.Persistence;
    using Logging;
    using Microsoft.EntityFrameworkCore;
    using Backend.Fx.Patterns.DependencyInjection;

    public abstract class DatabaseManager<TDbContext> : IDatabaseManager where TDbContext : DbContext
    {
        private readonly ICompositionRoot _compositionRoot;
        private static readonly ILogger Logger = LogManager.Create<DatabaseManager<TDbContext>>();

        protected DatabaseManager(ICompositionRoot compositionRoot)
        {
            _compositionRoot = compositionRoot;
        }

        public bool DatabaseExists { get; protected set; }

        public void EnsureDatabaseExistence()
        {
            Logger.Info("Ensuring database existence");
            using (var dbContext = _compositionRoot.GetInstance<TDbContext>())
            {
                ExecuteCreationStrategy(dbContext);
            }

            EnsureSearchIndexExistence();
            EnsureSequenceExistence();

            DatabaseExists = true;
        }

        private void EnsureSearchIndexExistence()
        {
            Logger.Info("Ensuring existence of full text search indizes");

            using (var dbContext = _compositionRoot.GetInstance<TDbContext>())
            {
                foreach (var fullTextSearchIndexType in _compositionRoot.GetInstances<IFullTextSearchIndex>())
                {
                    fullTextSearchIndexType.EnsureIndex(dbContext);
                }
            }
        }

        private void EnsureSequenceExistence()
        {
            Logger.Info("Ensuring existence of sequences");
            foreach (var sequence in _compositionRoot.GetInstances<ISequence>())
            {
                sequence.EnsureSequence(_compositionRoot.GetInstance<IDbConnection>());
            }
        }

        protected abstract void ExecuteCreationStrategy(DbContext dbContext);

        public virtual void DeleteDatabase()
        {
            using (var dbContext = _compositionRoot.GetInstance<TDbContext>())
            {
                Logger.Warn("Database is being deleted!");
                dbContext.Database.EnsureDeleted();
            }

            DatabaseExists = false;
        }
    }
}
