namespace Backend.Fx.EfCorePersistence
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Environment.Persistence;
    using Logging;
    using Microsoft.EntityFrameworkCore;
    using Mssql;

    public abstract class DatabaseManager<TDbContext> : IDatabaseManager where TDbContext : DbContext
    {
        private static readonly ILogger Logger = LogManager.Create<DatabaseManager<TDbContext>>();

        protected DatabaseManager(DbContextOptions<TDbContext> dbContextOptions)
        {
            DbContextOptions = dbContextOptions;
        }

        public bool DatabaseExists { get; protected set; }

        protected DbContextOptions<TDbContext> DbContextOptions { get; }

        public void EnsureDatabaseExistence()
        {
            using (var dbContext = DbContextOptions.CreateDbContext())
            {
                ExecuteCreationStrategy(dbContext);
            }

            EnsureSequenceExistence();
            EnsureSearchIndexExistence();

            DatabaseExists = true;
        }

        private void EnsureSearchIndexExistence()
        {
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

        private void EnsureSequenceExistence()
        {
            var sqlSequenceHiLoIdGeneratorTypes = typeof(TDbContext)
                    .GetTypeInfo()
                    .Assembly
                    .ExportedTypes
                    .Select(t => t.GetTypeInfo())
                    .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType && typeof(SqlSequenceHiLoIdGenerator).GetTypeInfo().IsAssignableFrom(t));

            foreach (var sqlSequenceHiLoIdGeneratorType in sqlSequenceHiLoIdGeneratorTypes)
            {
                SqlSequenceHiLoIdGenerator sqlSequenceHiLoIdGenerator = (SqlSequenceHiLoIdGenerator) Activator.CreateInstance(sqlSequenceHiLoIdGeneratorType.AsType(), DbContextOptions);
                sqlSequenceHiLoIdGenerator.EnsureSqlSequenceExistence();
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
