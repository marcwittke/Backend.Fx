using System;
using System.Data;
using Backend.Fx.Logging;

namespace Backend.Fx.Patterns.Transactions
{
    /// <summary>
    /// wraps an underlying database transaction. In combination with a injection container, access to
    /// the current transaction can be gained by means of this interface.
    /// </summary>
    public interface ITransactionContext
    {
        IDbTransaction CurrentTransaction { get; }
        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
        void SetIsolationLevel(IsolationLevel isolationLevel);
    }

    public class TransactionContext : ITransactionContext, IDisposable
    {
        private static readonly ILogger Logger = LogManager.Create<TransactionContext>();
        private readonly bool _shouldHandleConnectionState;
        private IsolationLevel _isolationLevel = IsolationLevel.Unspecified;
        private IDisposable _transactionLifetimeLogger;

        public TransactionContext(IDbConnection connection)
        {
            Connection = connection;
            ConnectionState state = Connection.State;
            switch (state)
            {
                case ConnectionState.Closed:
                    _shouldHandleConnectionState = true;
                    break;
                case ConnectionState.Open:
                    _shouldHandleConnectionState = false;
                    break;
                default:
                    throw new InvalidOperationException($"A connection provided to the TransactionContext must either be closed or open, but must not be {state}");
            }
        }

        public IDbConnection Connection { get; }

        public IDbTransaction CurrentTransaction { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void BeginTransaction()
        {
            if (_shouldHandleConnectionState)
            {
                Logger.Debug("Opening connection");
                Connection.Open();
            }

            Logger.Debug("Beginning transaction");
            CurrentTransaction = Connection.BeginTransaction(_isolationLevel);
            _transactionLifetimeLogger = Logger.DebugDuration("Transaction open");
        }

        public void CommitTransaction()
        {
            Logger.Debug("Committing transaction");
            CurrentTransaction.Commit();
            CurrentTransaction.Dispose();
            CurrentTransaction = null;
            _transactionLifetimeLogger?.Dispose();
            _transactionLifetimeLogger = null;
            if (_shouldHandleConnectionState)
            {
                Logger.Debug("Closing connection");
                Connection.Close();
            }
        }

        public void RollbackTransaction()
        {
            Logger.Debug("rolling back transaction");
            CurrentTransaction.Rollback();
            CurrentTransaction.Dispose();
            CurrentTransaction = null;
            _transactionLifetimeLogger?.Dispose();
            _transactionLifetimeLogger = null;
            if (_shouldHandleConnectionState)
            {
                Connection.Close();
            }
        }

        public void SetIsolationLevel(IsolationLevel isolationLevel)
        {
            if (CurrentTransaction != null)
            {
                throw new InvalidOperationException("Isolation level cannot be changed after the transaction has been started");
            }

            _isolationLevel = isolationLevel;
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    CurrentTransaction?.Dispose();
                    CurrentTransaction = null;
                    if (_shouldHandleConnectionState && Connection.State == ConnectionState.Open)
                    {
                        Connection.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Dispose failed");
                }

                _transactionLifetimeLogger?.Dispose();
                CurrentTransaction?.Dispose();
            }
        }
    }
}