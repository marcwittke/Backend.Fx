using System.Security.Principal;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.Transactions;

namespace Backend.Fx.Patterns.UnitOfWork
{
    /// <summary>
    /// Enriches the unit of work to use a database transaction during lifetime. The transaction gets started, before IUnitOfWork.Begin()
    /// is being called and gets committed after IUnitOfWork.Complete() is being called.
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

        public virtual void Begin()
        {
            TransactionContext.BeginTransaction();
            UnitOfWork.Begin();
        }

        public virtual void Complete()
        {
            UnitOfWork.Complete();
            TransactionContext.CommitTransaction();
        }
    }
}