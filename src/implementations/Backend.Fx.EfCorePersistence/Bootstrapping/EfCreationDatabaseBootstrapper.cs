﻿using Backend.Fx.Logging;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence.Bootstrapping
{
    public class EfCreationDatabaseBootstrapper<TDbContext> : EfDatabaseBootstrapper<TDbContext> where TDbContext : DbContext
    {
        private static readonly ILogger Logger = LogManager.Create<EfCreationDatabaseBootstrapper<TDbContext>>();

        public EfCreationDatabaseBootstrapper(TDbContext dbContext, IDatabaseBootstrapperInstanceProvider instanceProvider) 
            : base(dbContext, instanceProvider)
        {}

        protected override void ExecuteCreationStrategy(TDbContext dbContext)
        {
            Logger.Info("Creating database using the current schema version. This database won't be migratable.");
            dbContext.Database.EnsureCreated();
        }
    }
}