using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using Backend.Fx.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Backend.Fx.EfCorePersistence
{
    public class EfTransactionManager : IDisposable
    {
        private static readonly ILogger Logger = LogManager.Create<EfTransactionManager>();
        private readonly IDbConnection _dbConnection;
        private readonly DbContext _dbContext;
        private IDisposable _transactionLifetimeLogger;
        private IDbTransaction _currentTransaction;

        public EfTransactionManager(IDbConnection dbConnection, DbContext dbContext)
        {
            _dbConnection = dbConnection;
            _dbContext = dbContext;
        }

        public void Begin()
        {
            _dbConnection.Open();
            _currentTransaction = _dbConnection.BeginTransaction();
            _dbContext.Database.UseTransaction((DbTransaction)_currentTransaction);
            _transactionLifetimeLogger = Logger.DebugDuration("Transaction open");
        }

        public void Rollback()
        {
            Logger.Info("Rolling back transaction");
            try
            {
                _currentTransaction?.Rollback();
                _currentTransaction?.Dispose();
                _currentTransaction = null;
                if (_dbConnection.State == ConnectionState.Open)
                {
                    _dbConnection.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Rollback failed");
            }
            _transactionLifetimeLogger?.Dispose();
            _transactionLifetimeLogger = null;
        }

        public void Commit()
        {
            _currentTransaction.Commit();
            _currentTransaction.Dispose();
            _currentTransaction = null;
            _transactionLifetimeLogger?.Dispose();
            _transactionLifetimeLogger = null;
            _dbConnection.Close();
        }

        public void ResetTransactions()
        {
            // big fat HACK: until EF Core allows to change the transaction and/or connection on an existing instance of DbContext...
            RelationalConnection relationalConnection = (RelationalConnection)_dbContext.Database.GetService<IDbContextTransactionManager>();
            MethodInfo methodInfo = typeof(RelationalConnection).GetMethod("ClearTransactions", BindingFlags.Instance | BindingFlags.NonPublic);
            Debug.Assert(methodInfo != null, nameof(methodInfo) + " != null");
            methodInfo.Invoke(relationalConnection, new object[0]);
        }

        public void Dispose()
        {
            _transactionLifetimeLogger?.Dispose();
            _currentTransaction?.Dispose();
        }
    }
}