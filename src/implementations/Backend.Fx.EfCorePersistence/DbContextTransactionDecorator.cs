using System;
using System.Data.Common;
using System.Security.Principal;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.Transactions;
using Backend.Fx.Patterns.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence
{
    /// <summary>
    /// Makes sure the DbContext gets enlisted in a transaction. The transaction gets started, before IUnitOfWork.Begin() is being called
    /// and gets committed after IUnitOfWork.Complete() is being called
    /// </summary>
    public class DbContextTransactionDecorator : IUnitOfWork
    {
        public DbContextTransactionDecorator(ITransactionContext transactionContext, DbContext dbContext, IUnitOfWork unitOfWork)
        {
            TransactionContext = transactionContext;
            DbContext = dbContext;
            UnitOfWork = unitOfWork;
        }

        public IUnitOfWork UnitOfWork { get; }

        public ITransactionContext TransactionContext { get; }
        
        public DbContext DbContext { get; }

        public ICurrentTHolder<IIdentity> IdentityHolder => UnitOfWork.IdentityHolder;

        public void Begin()
        {
            TransactionContext.BeginTransaction();
            if (DbContext.Database.GetDbConnection() != TransactionContext.CurrentTransaction.Connection)
            {
                throw new InvalidOperationException("The connection used by the DbContext instance does not equal the connection of the transaction context");
            }
            DbContext.Database.UseTransaction((DbTransaction) TransactionContext.CurrentTransaction);
            UnitOfWork.Begin();
        }

        public void Complete()
        {
            UnitOfWork.Complete();
            TransactionContext.CommitTransaction();
        }

        public void Dispose()
        { }
    }
}