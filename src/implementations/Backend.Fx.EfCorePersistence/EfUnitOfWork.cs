namespace Backend.Fx.EfCorePersistence
{
    using System;
    using System.Security.Principal;
    using System.Data;
    using Environment.DateAndTime;
    using Logging;
    using Microsoft.EntityFrameworkCore;
    using Patterns.DependencyInjection;
    using Patterns.EventAggregation.Domain;
    using Patterns.EventAggregation.Integration;
    using Patterns.UnitOfWork;

    public class EfUnitOfWork : UnitOfWork, ICanInterruptTransaction
    {
        private readonly IDbConnection _dbConnection;
        private static readonly ILogger Logger = LogManager.Create<EfUnitOfWork>();
        private IDisposable _transactionLifetimeLogger;
        private IDbTransaction _currentTransaction;

        public EfUnitOfWork(IClock clock, ICurrentTHolder<IIdentity> identityHolder, IDomainEventAggregator eventAggregator, IEventBusScope eventBusScope, DbContext dbContext, IDbConnection dbConnection)
            : base(clock, identityHolder, eventAggregator, eventBusScope)
        {
            _dbConnection = dbConnection;
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
            _currentTransaction.Commit();
            _currentTransaction.Dispose();
            _currentTransaction = null;
            _transactionLifetimeLogger?.Dispose();
            _transactionLifetimeLogger = null;
        }

        protected override void Rollback()
        {
            Logger.Info("Rolling back transaction");
            try
            {
                _currentTransaction?.Rollback();
                _currentTransaction?.Dispose();
                _currentTransaction = null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Rollback failed");
            }
            _transactionLifetimeLogger?.Dispose();
            _transactionLifetimeLogger = null;
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
            _currentTransaction = _dbConnection.BeginTransaction();
            _transactionLifetimeLogger = Logger.DebugDuration("Transaction open");
        }
    }
}
