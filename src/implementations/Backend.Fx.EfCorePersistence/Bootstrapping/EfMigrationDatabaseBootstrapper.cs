using System;
using Backend.Fx.Logging;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence.Bootstrapping
{
    public class EfMigrationDatabaseBootstrapper<TDbContext> : EfDatabaseBootstrapper<TDbContext> where TDbContext : DbContext
    {
        private static readonly ILogger Logger = LogManager.Create<EfMigrationDatabaseBootstrapper<TDbContext>>();

        public EfMigrationDatabaseBootstrapper(Func<TDbContext> dbContextFactory, IDatabaseBootstrapperInstanceProvider instanceProvider)
            : base(dbContextFactory, instanceProvider)
        {}

        protected override void ExecuteCreationStrategy(TDbContext dbContext)
        {
            Logger.Info("Migrating database to latest schema version");
            dbContext.Database.Migrate();
        }
    }
}