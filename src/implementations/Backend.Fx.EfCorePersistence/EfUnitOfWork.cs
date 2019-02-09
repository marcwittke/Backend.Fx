using System;
using System.Data;
using System.Security.Principal;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.Patterns.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence
{
    public class EfUnitOfWork : UnitOfWork, ICanInterruptTransaction
    {
        private readonly EfTransactionManager _transactionManager;
        
        public EfUnitOfWork(IClock clock, ICurrentTHolder<IIdentity> identityHolder, IDomainEventAggregator eventAggregator,
            IEventBusScope eventBusScope, DbContext dbContext, IDbConnection dbConnection)
            : base(clock, identityHolder, eventAggregator, eventBusScope)
        {
            DbContext = dbContext;
            _transactionManager = new EfTransactionManager(dbConnection, dbContext);
        }

        public DbContext DbContext { get; }

        public override void Begin()
        {
            base.Begin();
            _transactionManager.Begin();
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
            _transactionManager.Commit();
        }

        protected override void Rollback()
        {
            _transactionManager.Rollback();
        }

        /// <inheritdoc />
        public void CompleteCurrentTransaction_InvokeAction_BeginNewTransaction(Action action)
        {
            Commit();
            action.Invoke();
            _transactionManager.ResetTransactions();
            _transactionManager.Begin();
        }

        /// <inheritdoc />
        public T CompleteCurrentTransaction_InvokeFunction_BeginNewTransaction<T>(Func<T> func)
        {
            Commit();
            T result = func.Invoke();
            _transactionManager.ResetTransactions();
            _transactionManager.Begin();
            return result;
        }

        /// <inheritdoc />
        public void CompleteCurrentTransaction_BeginNewTransaction()
        {
            Commit();
            _transactionManager.ResetTransactions();
            _transactionManager.Begin();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _transactionManager?.Dispose();
            }
        }
    }
}
