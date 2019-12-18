using System;
using System.Data;
using System.Security.Principal;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Patterns.UnitOfWork
{
    public class UnitOfWorkTransactionDecorator : IUnitOfWork
    {
        public UnitOfWorkTransactionDecorator(IDbConnection dbConnection, IUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
            TransactionContext = new TransactionContext(dbConnection);
        }

        public IUnitOfWork UnitOfWork { get; }

        public TransactionContext TransactionContext { get; }

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


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnitOfWork?.Dispose();
                TransactionContext?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}