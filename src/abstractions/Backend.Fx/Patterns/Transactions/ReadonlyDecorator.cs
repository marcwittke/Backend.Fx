using System.Data;

namespace Backend.Fx.Patterns.Transactions
{
    public class ReadonlyDecorator : ITransactionContext
    {
        private readonly ITransactionContext _transactionContext;

        public ReadonlyDecorator(ITransactionContext transactionContext)
        {
            _transactionContext = transactionContext;
        }

        public void BeginTransaction() => _transactionContext.BeginTransaction();

        public void CommitTransaction()
        {
            RollbackTransaction();
        }

        public void RollbackTransaction() => _transactionContext.RollbackTransaction();
        

        public IDbTransaction CurrentTransaction => _transactionContext.CurrentTransaction;
    }
}