using System;
using System.Data;
using Backend.Fx.EfCorePersistence.Bootstrapping;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.IdGeneration;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.EfCorePersistence.Postgres
{
    public abstract class PostgresSequence : ISequence
    {
        private static readonly ILogger Logger = Log.Create<PostgresSequence>();
        private readonly IDbConnectionFactory _dbConnectionFactory;

        protected PostgresSequence(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        protected abstract string SequenceName { get; }
        protected abstract string SchemaName { get; }

        public void EnsureSequence()
        {
            Logger.LogInformation("Ensuring existence of postgres sequence {SchemaName}.{SequenceName}", SchemaName, SequenceName);

            using (IDbConnection dbConnection = _dbConnectionFactory.Create())
            {
                dbConnection.Open();
                bool sequenceExists;
                using (IDbCommand command = dbConnection.CreateCommand())
                {
                    command.CommandText = $"SELECT count(*) FROM information_schema.sequences WHERE sequence_name = '{SequenceName}' AND sequence_schema = '{SchemaName}'";
                    sequenceExists = (long) command.ExecuteScalar() == 1L;
                }

                if (sequenceExists)
                {
                    Logger.LogInformation("Sequence {SchemaName}.{SequenceName} exists", SchemaName, SequenceName);
                }
                else
                {
                    Logger.LogInformation(
                        "Sequence {SchemaName}.{SequenceName} does not exist yet and will be created now",
                        SchemaName,
                        SequenceName);
                    using (IDbCommand cmd = dbConnection.CreateCommand())
                    {
                        cmd.CommandText = $"CREATE SEQUENCE {SchemaName}.{SequenceName} START WITH 1 INCREMENT BY {Increment}";
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
                using (IDbCommand command = dbConnection.CreateCommand())
                {
                    command.CommandText = $"SELECT nextval('{SchemaName}.{SequenceName}');";
                    nextValue = Convert.ToInt32(command.ExecuteScalar());
                    Logger.LogDebug("{SchemaName}.{SequenceName} served {2} as next value", SchemaName, SequenceName, nextValue);
                }

                return nextValue;
            }
        }

        public abstract int Increment { get; }
    }
}