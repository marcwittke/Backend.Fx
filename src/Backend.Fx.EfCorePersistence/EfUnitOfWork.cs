namespace Backend.Fx.EfCorePersistence
{
    using System;
    using System.Security.Principal;
    using Environment.DateAndTime;
    using Logging;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using Patterns.DependencyInjection;
    using Patterns.EventAggregation.Domain;
    using Patterns.EventAggregation.Integration;
    using Patterns.UnitOfWork;

    public class EfUnitOfWork : UnitOfWork, ICanInterruptTransaction
    {
        private static readonly ILogger Logger = LogManager.Create<EfUnitOfWork>();
        private IDisposable transactionLifetimeLogger;
        private IDbContextTransaction currentTransaction;

        public EfUnitOfWork(IClock clock, ICurrentTHolder<IIdentity> identityHolder, IDomainEventAggregator eventAggregator, IEventBusScope eventBusScope, DbContext dbContext)
            : base(clock, identityHolder, eventAggregator, eventBusScope)
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

        /// <inheritdoc />
        public void CompleteCurrentTransaction_InvokeAction_BeginNewTransaction(Action action)
        {
            Commit();
            action.Invoke();
            BeginTransaction();
        }

        /// <inheritdoc />
        public T CompleteCurrentTransaction_InvokeFunction_BeginNewTransaction<T>(Func<T> func)
        {
            Commit();
            T result = func.Invoke();
            BeginTransaction();
            return result;
        }

        /// <inheritdoc />
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
