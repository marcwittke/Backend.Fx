using System;
using System.Data;
using Backend.Fx.EfCorePersistence.Bootstrapping;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.IdGeneration;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.EfCorePersistence.Mssql
{
    public abstract class MsSqlSequence : ISequence
    {
        private static readonly ILogger Logger = LogManager.Create<MsSqlSequence>();
        private readonly IDbConnectionFactory _dbConnectionFactory;

        protected MsSqlSequence(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        protected abstract string SequenceName { get; }
        protected virtual string SchemaName { get; } = "dbo";

        public void EnsureSequence()
        {
            Logger.LogInformation("Ensuring existence of mssql sequence {SchemaName}.{SequenceName}", SchemaName, SequenceName);
            using (IDbConnection dbConnection = _dbConnectionFactory.Create())
            {
                dbConnection.Open();
                bool sequenceExists;
                using (IDbCommand cmd = dbConnection.CreateCommand())
                {
                    cmd.CommandText = $"SELECT count(*) FROM sys.sequences seq join sys.schemas s on s.schema_id  = seq.schema_id WHERE seq.name = '{SequenceName}' and s.name = '{SchemaName}'";
                    sequenceExists = (int) cmd.ExecuteScalar() == 1;
                }

                if (sequenceExists)
                {
                    Logger.LogInformation("Sequence {SchemaName}.{SequenceName} exists", SchemaName, SequenceName);
                }
                else
                {
                    Logger.LogInformation("Sequence {SchemaName}.{SequenceName} does not exist yet and will be created now", SchemaName, SequenceName);
                    using (IDbCommand cmd = dbConnection.CreateCommand())
                    {
                        cmd.CommandText = $"CREATE SEQUENCE [{SchemaName}].[{SequenceName}] START WITH 1 INCREMENT BY {Increment}";
                        cmd.ExecuteNonQuery();
                        Logger.LogInformation("Sequence {SchemaName}.{SequenceName} created", SchemaName, SequenceName);
                    }
                }
            }
        }

        public int GetNextValue()
        {
            using (IDbConnection dbConnection = _dbConnectionFactory.Create())
            {
                dbConnection.Open();
                int nextValue;
                using (IDbCommand selectNextValCommand = dbConnection.CreateCommand())
                {
                    selectNextValCommand.CommandText = $"SELECT next value FOR {SchemaName}.{SequenceName}";
                    nextValue = Convert.ToInt32(selectNextValCommand.ExecuteScalar());
                    Logger.LogDebug("{SchemaName}.{SequenceName} served {NextValue} as next value", SchemaName, SequenceName, nextValue);
                }

                return nextValue;
            }
        }

        public abstract int Increment { get; }
    }
}