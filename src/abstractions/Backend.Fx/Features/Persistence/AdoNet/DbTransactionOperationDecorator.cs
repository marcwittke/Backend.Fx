using System;
using System.Data;
using System.Threading.Tasks;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Logging;
using Backend.Fx.Util;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Features.Persistence.AdoNet
{
    /// <summary>
    /// Enriches the operation to use a database transaction during lifetime. The transaction gets started, before IOperation.Begin()
    /// is being called and gets committed after IOperation.Complete() is being called.
    /// </summary>
    [PublicAPI]
    public class DbTransactionOperationDecorator : IOperation
    {
        private static readonly ILogger Logger = Log.Create<DbTransactionOperationDecorator>();
        private readonly IDbConnection _dbConnection;
        private readonly ICurrentTHolder<IDbTransaction> _currentTransactionHolder;
        private readonly IOperation _operation;
        private bool _shouldHandleConnectionState;
        private IsolationLevel _isolationLevel = IsolationLevel.Unspecified;
        private IDisposable _transactionLifetimeLogger;
        private TxState _state = TxState.NotStarted;
        
        public DbTransactionOperationDecorator(IDbConnection dbConnection, ICurrentTHolder<IDbTransaction> currentTransactionHolder, IOperation operation)
        {
            _dbConnection = dbConnection;
            _currentTransactionHolder = currentTransactionHolder;
            _operation = operation;
        }


        public virtual async Task BeginAsync(IServiceScope serviceScope)
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
            _currentTransactionHolder.ReplaceCurrent(_dbConnection.BeginTransaction(_isolationLevel));
            _transactionLifetimeLogger = Logger.LogDebugDuration("Transaction open", "Transaction terminated");
            _state = TxState.Active;
            await _operation.BeginAsync(serviceScope).ConfigureAwait(false);
        }

        public async Task CompleteAsync()
        {
            await _operation.CompleteAsync().ConfigureAwait(false);

            if (_state != TxState.Active)
            {
                throw new InvalidOperationException($"A transaction cannot be committed when it is {_state}.");
            }

            Logger.LogDebug("Committing transaction");
            _currentTransactionHolder.Current.Commit();
            _currentTransactionHolder.Current.Dispose();
            _currentTransactionHolder.ReplaceCurrent(null);
            _transactionLifetimeLogger?.Dispose();
            _transactionLifetimeLogger = null;
            if (_shouldHandleConnectionState)
            {
                Logger.LogDebug("Closing connection");
                _dbConnection.Close();
            }

            _state = TxState.Committed;
        }

        public async Task CancelAsync()
        {
            Logger.LogDebug("rolling back transaction");
            if (_state != TxState.Active)
            {
                throw new InvalidOperationException($"Cannot roll back a transaction that is {_state}");
            }
            
            await _operation.CancelAsync().ConfigureAwait(false);

            _currentTransactionHolder.Current.Rollback();
            _currentTransactionHolder.Current.Dispose();
            _currentTransactionHolder.ReplaceCurrent(null);

            _transactionLifetimeLogger?.Dispose();
            _transactionLifetimeLogger = null;
            if (_shouldHandleConnectionState)
            {
                _dbConnection.Close();
            }

            _state = TxState.RolledBack;
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