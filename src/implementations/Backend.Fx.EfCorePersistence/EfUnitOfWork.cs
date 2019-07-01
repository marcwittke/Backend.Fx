using System;
using System.Data;
using System.Data.Common;
using System.Security.Principal;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.Patterns.UnitOfWork;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence
{
    public class EfUnitOfWork : UnitOfWork, ICanInterruptTransaction
    {
        private static readonly ILogger Logger = LogManager.Create<EfUnitOfWork>();
        private readonly IDbConnection _dbConnection;
        private IDisposable _transactionLifetimeLogger;
        private IDbTransaction _currentTransaction;
        [CanBeNull] private DbContext _dbContext;

        public EfUnitOfWork(IClock clock, ICurrentTHolder<IIdentity> identityHolder, IDomainEventAggregator eventAggregator,
            IEventBusScope eventBusScope, [CanBeNull] DbContext dbContext, IDbConnection dbConnection)
            : base(clock, identityHolder, eventAggregator, eventBusScope)
        {
            _dbConnection = dbConnection;
            _dbContext = dbContext;
        }

        public virtual DbContext DbContext
        {
            get => _dbContext ?? throw new InvalidOperationException("This EfUnitOfWork does not have a DbContext yet. You might either make sure a proper DbContext gets injected or the DbContext is initialized later using a derived class");
            protected set => _dbContext = value;
        }

        public override void Begin()
        {
            base.Begin();
            _dbConnection.Open();
            _currentTransaction = _dbConnection.BeginTransaction();
            DbContext.Database.UseTransaction((DbTransaction)_currentTransaction);
            _transactionLifetimeLogger = Logger.DebugDuration("Transaction open");
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
            _currentTransaction.Commit();
            _currentTransaction.Dispose();
            _currentTransaction = null;
            _transactionLifetimeLogger?.Dispose();
            _transactionLifetimeLogger = null;
            _dbConnection.Close();
        }

        protected override void Rollback()
        {
            Logger.Info("Rolling back transaction");
            try
            {
                _currentTransaction?.Rollback();
                _currentTransaction?.Dispose();
                _currentTransaction = null;
                if (_dbConnection.State == ConnectionState.Open)
                {
                    _dbConnection.Close();
                }
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
            Flush();
            Commit();
            action.Invoke();
            DbContext.ResetTransactions();
            Begin();
        }

        /// <inheritdoc />
        public T CompleteCurrentTransaction_InvokeFunction_BeginNewTransaction<T>(Func<T> func)
        {
            Flush();
            Commit();
            T result = func.Invoke();
            DbContext.ResetTransactions();
            Begin();
            return result;
        }

        /// <inheritdoc />
        public void CompleteCurrentTransaction_BeginNewTransaction()
        {
            Flush();
            Commit();
            DbContext.ResetTransactions();
            Begin();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _transactionLifetimeLogger?.Dispose();
                _currentTransaction?.Dispose();
            }
        }
    }
}
