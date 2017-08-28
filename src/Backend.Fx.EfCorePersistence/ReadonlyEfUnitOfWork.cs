namespace Backend.Fx.EfCorePersistence
{
    using System;
    using Logging;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using Patterns.UnitOfWork;

    public class ReadonlyEfUnitOfWork : ReadonlyUnitOfWork
    {
        private static readonly ILogger Logger = LogManager.Create<EfUnitOfWork>();
        private readonly DbContext dbContext;
        private IDisposable transactionLifetimeLogger;
        private IDbContextTransaction currentTransaction;

        public ReadonlyEfUnitOfWork(DbContext dbContext)
        {
            this.dbContext = dbContext;
            this.dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            this.dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public override void Begin()
        {
            base.Begin();
            BeginTransaction();
        }
        
        protected override void Rollback()
        {
            Logger.Info("Rolling back transaction");
            try
            {
                currentTransaction?.Rollback();
                currentTransaction?.Dispose();
                currentTransaction = null;
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Rollback failed");
            }
            transactionLifetimeLogger?.Dispose();
            transactionLifetimeLogger = null;
        }

        private void BeginTransaction()
        {
            currentTransaction = dbContext.Database.BeginTransaction();
            transactionLifetimeLogger = Logger.InfoDuration("Transaction open");
        }
    }
}
