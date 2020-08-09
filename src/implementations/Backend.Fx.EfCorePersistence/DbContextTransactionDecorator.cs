using System;
using System.Data.Common;
using Backend.Fx.Patterns.Transactions;
using Backend.Fx.Patterns.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence
{
    /// <summary>
    ///     Makes sure the DbContext gets enlisted in a transaction.
    /// </summary>
    public class DbContextTransactionDecorator : DbTransactionDecorator
    {
        public DbContextTransactionDecorator(ITransactionContext transactionContext, DbContext dbContext, IUnitOfWork unitOfWork)
            : base(transactionContext, unitOfWork)
        {
            DbContext = dbContext;
        }

        public DbContext DbContext { get; }

        public override void Begin()
        {
            base.Begin();
            if (DbContext.Database.GetDbConnection() != TransactionContext.CurrentTransaction.Connection)
                throw new InvalidOperationException("The connection used by the DbContext instance does not equal the connection of the transaction context");
            DbContext.Database.UseTransaction((DbTransaction) TransactionContext.CurrentTransaction);
        }
    }
}