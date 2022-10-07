using System;
using System.Data;
using System.Threading.Tasks;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Features.Persistence.AdoNet
{
    [UsedImplicitly]
    public class DbConnectionOperationDecorator : IOperation
    {
        private static readonly ILogger Logger = Log.Create<DbConnectionOperationDecorator>();
        private IDisposable _connectionLifetimeLogger;
        private readonly IOperation _operation;
        private readonly IDbConnection _dbConnection;

        public DbConnectionOperationDecorator(IDbConnection dbConnection, IOperation operation)
        {
            _dbConnection = dbConnection;
            _operation = operation;
        }

        public async Task BeginAsync(IServiceScope serviceScope)
        {
            Logger.LogDebug("Opening database connection");
            _dbConnection.Open();
            _connectionLifetimeLogger = Logger.LogDebugDuration("Database connection open", "Database connection closed");
            await _operation.BeginAsync(serviceScope).ConfigureAwait(false);
        }

        public async Task CompleteAsync()
        {
            await _operation.CompleteAsync().ConfigureAwait(false);
            Logger.LogDebug("Closing database connection");
            _dbConnection.Close();
            _connectionLifetimeLogger?.Dispose();
        }

        public async Task CancelAsync()
        {
            await _operation.CancelAsync().ConfigureAwait(false);
            Logger.LogDebug("Closing database connection");
            _dbConnection.Close();
            _connectionLifetimeLogger?.Dispose();
            
            // note: we do not dispose the DbConnection here, because we did not instantiate it. Disposing is always
            // up to the creator of the instance, that is in this case the injection container.
        }
    }
}