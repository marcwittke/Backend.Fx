using System.Data;
using Backend.Fx.Logging;

namespace Backend.Fx.Patterns.Transactions
{
    public class ReadonlyDecorator : ITransactionContext
    {
        private static readonly ILogger Logger = LogManager.Create<ReadonlyDecorator>();
        
        private readonly ITransactionContext _transactionContext;

        public ReadonlyDecorator(ITransactionContext transactionContext)
        {
            Logger.Debug("Making this transaction context readonly");
            _transactionContext = transactionContext;
        }

        public void BeginTransaction() => _transactionContext.BeginTransaction();

        public void CommitTransaction()
        {
            Logger.Debug("Committing transaction is intercepted and replaced with rollback transaction to ensure readonly behavior");
            RollbackTransaction();
        }

        public void RollbackTransaction() => _transactionContext.RollbackTransaction();
        

        public IDbTransaction CurrentTransaction => _transactionContext.CurrentTransaction;
    }
}