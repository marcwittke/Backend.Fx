using System;
using System.Data;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Environment.Persistence
{
    /// <summary>
    /// Enriches the operation to use a database transaction during lifetime. The transaction gets started, before IOperation.Begin()
    /// is being called and gets committed after IOperation.Complete() is being called.
    /// </summary>
    public class DbTransactionOperationDecorator : IOperation
    {
        private static readonly ILogger Logger = Log.Create<DbTransactionOperationDecorator>();
        private readonly IDbConnection _dbConnection;
        private readonly IOperation _operation;
        private bool _shouldHandleConnectionState;
        private IsolationLevel _isolationLevel = IsolationLevel.Unspecified;
        private IDisposable _transactionLifetimeLogger;
        private TxState _state = TxState.NotStarted;
        
        public DbTransactionOperationDecorator(IDbConnection dbConnection, IOperation operation)
        {
            _dbConnection = dbConnection;
            _operation = operation;
        }


        public virtual void Begin()
        {
            if (_state != TxState.NotStarted)
            {
                throw new InvalidOperationException("A Transaction has been started by this operation before.");
            }

            _shouldHandleConnectionState = ShouldHandleConnectionState();
            if (_shouldHandleConnectionState)
            {
                Logger.LogDebug("Opening connection");
                _dbConnection.Open();
            }

            Logger.LogDebug("Beginning transaction");
            CurrentTransaction = _dbConnection.BeginTransaction(_isolationLevel);
            _transactionLifetimeLogger = Logger.LogDebugDuration("Transaction open", "Transaction terminated");
            _state = TxState.Active;
            _operation.Begin();
        }

        public IDbTransaction CurrentTransaction { get; private set; }

        public void Complete()
        {
            if (_state != TxState.Active)
            {
                throw new InvalidOperationException($"A transaction cannot be committed when it is {_state}.");
            }

            Logger.LogDebug("Committing transaction");
            CurrentTransaction.Commit();
            CurrentTransaction.Dispose();
            CurrentTransaction = null;
            _transactionLifetimeLogger?.Dispose();
            _transactionLifetimeLogger = null;
            if (_shouldHandleConnectionState)
            {
                Logger.LogDebug("Closing connection");
                _dbConnection.Close();
            }

            _state = TxState.Committed;
            _operation.Complete();
        }

        public void Cancel()
        {
            Logger.LogDebug("rolling back transaction");
            if (_state != TxState.Active)
            {
                throw new InvalidOperationException($"Cannot roll back a transaction that is {_state}");
            }

            try
            {
                CurrentTransaction.Rollback();
            }
            catch (InvalidOperationException ioe)
            {
                Logger.LogError(ioe, "Failed to roll back transaction");                
            }
            
            CurrentTransaction.Dispose();
            CurrentTransaction = null;

            _transactionLifetimeLogger?.Dispose();
            _transactionLifetimeLogger = null;
            if (_shouldHandleConnectionState)
            {
                _dbConnection.Close();
            }

            _state = TxState.RolledBack;
            
            _operation.Cancel();
        }
        
        public void SetIsolationLevel(IsolationLevel isolationLevel)
        {
            if (_state != TxState.NotStarted)
            {
                throw new InvalidOperationException("Isolation level cannot be changed after the transaction has been started");
            }

            _isolationLevel = isolationLevel;
        }
        
        private bool ShouldHandleConnectionState()
        {
            switch (_dbConnection.State)
            {
                case ConnectionState.Closed:
                    return true;
                case ConnectionState.Open:
                    return false;
                default:
                    throw new InvalidOperationException($"A connection provided to the operation must either be closed or open, but must not be {_dbConnection.State}");
            }
        }
        
        private enum TxState
        {
            NotStarted,
            Active,
            Committed,
            RolledBack
        }
    }
}