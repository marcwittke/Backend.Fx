using System;
using System.Data;
using Backend.Fx.Logging;

namespace Backend.Fx.Patterns.UnitOfWork
{
    public class TransactionContext : IDisposable 
    {
        private static readonly ILogger Logger = LogManager.Create<TransactionContext>();
        private IDisposable _transactionLifetimeLogger;
        private readonly bool _shouldHandleConnectionState;
        
        public IDbConnection Connection { get; }
        
        public IDbTransaction CurrentTransaction { get; private set; }
        
        public TransactionContext(IDbConnection connection)
        {
            Connection = connection;
            ConnectionState connectionState = Connection.State;
            switch (connectionState)
            {
                case ConnectionState.Open:
                    _shouldHandleConnectionState = false;
                    break;
                case ConnectionState.Closed:
                    _shouldHandleConnectionState = true;
                    break;
                default:
                    throw new InvalidOperationException($"A connection provided to the TransactionContext must either be closed or open, but must not be {connectionState}");
            }
        }

        public void BeginTransaction()
        {
            if (_shouldHandleConnectionState)
            {
                Connection.Open();
            }
            CurrentTransaction = Connection.BeginTransaction();
            _transactionLifetimeLogger = Logger.DebugDuration("Transaction open");
        }
        
        public void CommitTransaction()
        {
            CurrentTransaction.Commit();
            CurrentTransaction.Dispose();
            CurrentTransaction = null;
            _transactionLifetimeLogger?.Dispose();
            _transactionLifetimeLogger = null;
            if (_shouldHandleConnectionState)
            {
                Connection.Close();
            }
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
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}