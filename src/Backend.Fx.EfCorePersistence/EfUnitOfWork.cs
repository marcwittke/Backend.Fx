namespace Backend.Fx.EfCorePersistence
{
    using System;
    using System.Security.Principal;
    using Environment.DateAndTime;
    using Logging;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using Patterns.UnitOfWork;

    public class EfUnitOfWork : UnitOfWork
    {
        private static readonly ILogger Logger = LogManager.Create<EfUnitOfWork>();
        private readonly DbContext dbContext;
        private IDisposable transactionLifetimeLogger;
        private IDbContextTransaction currentTransaction;

        public EfUnitOfWork(IClock clock, IIdentity identity, DbContext dbContext)
            : base(clock, identity)
        {
            this.dbContext = dbContext;
        }

        public override void Begin()
        {
            base.Begin();
            BeginTransaction();
        }

        public override void Flush()
        {
            base.Flush();
            dbContext.SaveChanges();
        }

        protected override void UpdateTrackingProperties(string userId, DateTime utcNow)
        {
            dbContext.UpdateTrackingProperties(userId, utcNow);
        }

        protected override void Commit()
        {
            Flush();
            currentTransaction.Commit();
            currentTransaction.Dispose();
            currentTransaction = null;
            transactionLifetimeLogger?.Dispose();
            transactionLifetimeLogger = null;
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
            transactionLifetimeLogger = Logger.DebugDuration("Transaction open");
        }
    }
}
