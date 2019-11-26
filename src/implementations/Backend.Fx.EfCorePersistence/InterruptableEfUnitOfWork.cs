using System;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.Patterns.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Backend.Fx.EfCorePersistence
{
    public class InterruptableEfUnitOfWork<TDbContext> : EfUnitOfWork<TDbContext>, ICanInterruptTransaction where TDbContext : DbContext
    {
        public InterruptableEfUnitOfWork(IClock clock, ICurrentTHolder<IIdentity> identityHolder, IDomainEventAggregator eventAggregator,
            IEventBusScope eventBusScope, Func<DbConnection, TDbContext> dbContextFactory, DbConnection dbConnection)
            : base(clock, identityHolder, eventAggregator, eventBusScope, dbContextFactory, dbConnection)
        { }
        
        /// <inheritdoc />
        public void CompleteCurrentTransaction_InvokeAction_BeginNewTransaction(Action action)
        {
            Flush();
            Commit();
            action.Invoke();
            ResetTransactions();
            Begin();
        }

        /// <inheritdoc />
        public T CompleteCurrentTransaction_InvokeFunction_BeginNewTransaction<T>(Func<T> func)
        {
            Flush();
            Commit();
            T result = func.Invoke();
            ResetTransactions();
            Begin();
            return result;
        }

        /// <inheritdoc />
        public void CompleteCurrentTransaction_BeginNewTransaction()
        {
            Flush();
            Commit();
            ResetTransactions();
            Begin();
        }
        
        private void ResetTransactions()
        {
            // big fat HACK: until EF Core allows to change the transaction and/or connection on an existing instance of DbContext...
            var relationalConnection = (RelationalConnection)DbContext.Database.GetService<IDbContextTransactionManager>();
            MethodInfo methodInfo = typeof(RelationalConnection).GetMethod("ClearTransactions", BindingFlags.Instance | BindingFlags.NonPublic);
            Debug.Assert(methodInfo != null, nameof(methodInfo) + " != null");
            methodInfo.Invoke(relationalConnection, new object[] {false});
        }
    }
}