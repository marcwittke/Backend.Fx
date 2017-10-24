namespace Backend.Fx.EfCorePersistence
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Environment.Persistence;
    using Logging;
    using Microsoft.EntityFrameworkCore;

    public abstract class DatabaseManager<TDbContext> : IDatabaseManager where TDbContext : DbContext
    {
        private static readonly ILogger Logger = LogManager.Create<DatabaseManager<TDbContext>>();

        protected DatabaseManager(DbContextOptions<TDbContext> dbContextOptions)
        {
            DbContextOptions = dbContextOptions;
        }

        public bool DatabaseExists { get; protected set; }

        public DbContextOptions<TDbContext> DbContextOptions { get; }

        public void EnsureDatabaseExistence()
        {
            Logger.Info("Ensuring database existence");
            using (var dbContext = DbContextOptions.CreateDbContext())
            {
                ExecuteCreationStrategy(dbContext);
            }

            EnsureSearchIndexExistence();

            DatabaseExists = true;
        }

        private void EnsureSearchIndexExistence()
        {
            Logger.Info("Ensuring existence of full text search indizes");
            var fullTextSearchIndexTypes = typeof(TDbContext)
                    .GetTypeInfo()
                    .Assembly
                    .ExportedTypes
                    .Select(t => t.GetTypeInfo())
                    .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType && typeof(IFullTextSearchIndex).GetTypeInfo().IsAssignableFrom(t));

            using (var dbContext = DbContextOptions.CreateDbContext())
            {
                foreach (var fullTextSearchIndexType in fullTextSearchIndexTypes)
                {
                    IFullTextSearchIndex fullTextSearchIndex = (IFullTextSearchIndex)Activator.CreateInstance(fullTextSearchIndexType.AsType());
                    fullTextSearchIndex.EnsureIndex(dbContext);
                }
            }
        }
        
        protected abstract void ExecuteCreationStrategy(DbContext dbContext);

        public virtual void DeleteDatabase()
        {
            using (var dbContext = DbContextOptions.CreateDbContext())
            {
                Logger.Warn("Database is being deleted!");
                dbContext.Database.EnsureDeleted();
            }

            DatabaseExists = false;
        }
    }
}
