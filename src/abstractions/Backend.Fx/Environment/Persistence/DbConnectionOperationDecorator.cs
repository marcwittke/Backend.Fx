using System;
using System.Data;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Environment.Persistence
{
    public class DbConnectionOperationDecorator : IOperation
    {
        private static readonly ILogger Logger = LogManager.Create<DbConnectionOperationDecorator>();
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
            Logger.Debug("Opening database connection");
            DbConnection.Open();
            _connectionLifetimeLogger = Logger.DebugDuration("Database connection open", "Database connection closed");
            Operation.Begin();
        }

        public void Complete()
        {
            Operation.Complete();
            Logger.Debug("Closing database connection");
            DbConnection.Close();
            _connectionLifetimeLogger?.Dispose();
        }

        public void Cancel()
        {
            Operation.Cancel();
        }
    }
}