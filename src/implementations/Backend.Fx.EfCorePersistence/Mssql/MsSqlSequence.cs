using System;
using System.Data;
using Backend.Fx.EfCorePersistence.Bootstrapping;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.IdGeneration;

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
            Logger.Info($"Ensuring existence of mssql sequence {SchemaName}.{SequenceName}");
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
                    Logger.Info($"Sequence {SchemaName}.{SequenceName} exists");
                }
                else
                {
                    Logger.Info($"Sequence {SchemaName}.{SequenceName} does not exist yet and will be created now");
                    using (IDbCommand cmd = dbConnection.CreateCommand())
                    {
                        cmd.CommandText = $"CREATE SEQUENCE {SchemaName}.{SequenceName} START WITH 1 INCREMENT BY {Increment}";
                        cmd.ExecuteNonQuery();
                        Logger.Info($"Sequence {SchemaName}.{SequenceName} created");
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
                    Logger.Debug($"{SchemaName}.{SequenceName} served {nextValue} as next value");
                }

                return nextValue;
            }
        }

        public abstract int Increment { get; }
    }
}