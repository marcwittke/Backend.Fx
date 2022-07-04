using System;
using System.Data;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Features.Persistence
{
    public class DbConnectionOperationDecorator : IOperation
    {
        private static readonly ILogger Logger = Log.Create<DbConnectionOperationDecorator>();
        private IDisposable _connectionLifetimeLogger;       
        public DbConnectionOperationDecorator(IDbConnection dbConnection, IOperation operation)
        {
            DbConnection = dbConnection;
            Operation = operation;
        }

        public IOperation Operation { get; }

        public IDbConnection DbConnection { get; }

        public void Begin()
        {
            Logger.LogDebug("Opening database connection");
            DbConnection.Open();
            _connectionLifetimeLogger = Logger.LogDebugDuration("Database connection open", "Database connection closed");
            Operation.Begin();
        }

        public void Complete()
        {
            Operation.Complete();
            Logger.LogDebug("Closing database connection");
            DbConnection.Close();
            _connectionLifetimeLogger?.Dispose();
        }

        public void Cancel()
        {
            Operation.Cancel();
            Logger.LogDebug("Closing database connection");
            DbConnection.Close();
            _connectionLifetimeLogger?.Dispose();
            
            // note: we do not dispose the DbConnection here, because we did not instantiate it. Disposing is always up to the creator of
            // the instance, that is in this case the injection container.
        }
    }
}