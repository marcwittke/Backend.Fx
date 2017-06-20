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

        protected DatabaseManager(DbContextOptions dbContextOptions)
        {
            DbContextOptions = dbContextOptions;
        }

        public bool DatabaseExists { get; protected set; }

        protected DbContextOptions DbContextOptions { get; }

        public void EnsureDatabaseExistence()
        {
            using (var dbContext = DbContextOptions.CreateDbContext<TDbContext>())
            {
                ExecuteCreationStrategy(dbContext);
            }

            var sqlSequenceHiLoIdGeneratorTypes = typeof(TDbContext)
                    .GetTypeInfo()
                    .Assembly
                    .ExportedTypes
                    .Select(t => t.GetTypeInfo())
                    .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType && typeof(SqlSequenceHiLoIdGenerator).GetTypeInfo().IsAssignableFrom(t));

            foreach (var sqlSequenceHiLoIdGeneratorType in sqlSequenceHiLoIdGeneratorTypes)
            {
                SqlSequenceHiLoIdGenerator sqlSequenceHiLoIdGenerator = (SqlSequenceHiLoIdGenerator)Activator.CreateInstance(sqlSequenceHiLoIdGeneratorType.AsType(), DbContextOptions);
                sqlSequenceHiLoIdGenerator.EnsureSqlSequenceExistence();
            }

            DatabaseExists = true;
        }

        protected abstract void ExecuteCreationStrategy(DbContext dbContext);

        public virtual void DeleteDatabase()
        {
            using (var dbContext = DbContextOptions.CreateDbContext<TDbContext>())
            {
                Logger.Warn("Database is being deleted!");
                dbContext.Database.EnsureDeleted();
            }

            DatabaseExists = false;
        }
    }
}
