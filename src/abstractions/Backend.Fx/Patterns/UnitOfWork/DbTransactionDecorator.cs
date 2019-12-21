using System.Security.Principal;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.Transactions;

namespace Backend.Fx.Patterns.UnitOfWork
{
    /// <summary>
    /// Enriches the unit of work to use a database transaction during lifetime. 
    /// </summary>
    public class DbTransactionDecorator : IUnitOfWork
    {
        public DbTransactionDecorator(ITransactionContext transactionContext, IUnitOfWork unitOfWork)
        {
            TransactionContext = transactionContext;
            UnitOfWork = unitOfWork;
        }

        public IUnitOfWork UnitOfWork { get; }

        public ITransactionContext TransactionContext { get; }

        public ICurrentTHolder<IIdentity> IdentityHolder => UnitOfWork.IdentityHolder;

        public void Begin()
        {
            TransactionContext.BeginTransaction();
            UnitOfWork.Begin();
        }

        public void Complete()
        {
            UnitOfWork.Complete();
            TransactionContext.CommitTransaction();
        }
    }
}