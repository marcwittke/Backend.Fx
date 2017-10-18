﻿namespace Backend.Fx.EfCorePersistence
{
    using System;
    using System.Security.Principal;
    using Environment.DateAndTime;
    using Logging;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using Patterns.UnitOfWork;

    public class EfUnitOfWork : UnitOfWork, ICanInterruptTransaction
    {
        private static readonly ILogger Logger = LogManager.Create<EfUnitOfWork>();
        private IDisposable transactionLifetimeLogger;
        private IDbContextTransaction currentTransaction;

        public EfUnitOfWork(IClock clock, IIdentity identity, DbContext dbContext)
            : base(clock, identity)
        {
            DbContext = dbContext;
        }

        public DbContext DbContext { get; }

        public override void Begin()
        {
            base.Begin();
            BeginTransaction();
        }

        public override void Flush()
        {
            base.Flush();
            DbContext.SaveChanges();
        }

        protected override void UpdateTrackingProperties(string userId, DateTime utcNow)
        {
            DbContext.UpdateTrackingProperties(userId, utcNow);
        }

        protected override void Commit()
        {
            DbContext.SaveChanges();
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

        public void CompleteCurrentTransaction_InvokeAction_BeginNewTransaction(Action action)
        {
            Commit();
            action.Invoke();
            BeginTransaction();
        }

        public T CompleteCurrentTransaction_InvokeFunction_BeginNewTransaction<T>(Func<T> func)
        {
            Commit();
            T result = func.Invoke();
            BeginTransaction();
            return result;
        }

        public void CompleteCurrentTransaction_BeginNewTransaction()
        {
            Commit();
            BeginTransaction();
        }

        private void BeginTransaction()
        {
            currentTransaction = DbContext.Database.BeginTransaction();
            transactionLifetimeLogger = Logger.DebugDuration("Transaction open");
        }
    }
}
