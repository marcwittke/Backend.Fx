using System;
using System.Data;
using Backend.Fx.EfCorePersistence.Bootstrapping;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.IdGeneration;

namespace Backend.Fx.EfCorePersistence.Oracle
{
    public abstract class OracleSequence : ISequence
    {
        private static readonly ILogger Logger = LogManager.Create<OracleSequence>();
        private readonly IDbConnectionFactory _dbConnectionFactory;

        protected OracleSequence(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        protected abstract string SequenceName { get; }
        protected abstract string SchemaName { get; }

        private string SchemaPrefix
        {
            get
            {
                if (string.IsNullOrEmpty(SchemaName)) return string.Empty;

                return SchemaName + ".";
            }
        }

        public void EnsureSequence()
        {
            Logger.Info($"Ensuring existence of oracle sequence {SchemaPrefix}{SequenceName}");

            using (IDbConnection dbConnection = _dbConnectionFactory.Create())
            {
                dbConnection.Open();
                bool sequenceExists;
                using (IDbCommand command = dbConnection.CreateCommand())
                {
                    command.CommandText = $"SELECT count(*) FROM user_sequences WHERE sequence_name = '{SequenceName}'";
                    sequenceExists = (decimal) command.ExecuteScalar() == 1;
                }

                if (sequenceExists)
                {
                    Logger.Info($"Sequence {SchemaPrefix}{SequenceName} exists");
                }
                else
                {
                    Logger.Info($"Sequence {SchemaPrefix}{SequenceName} does not exist yet and will be created now");
                    using (IDbCommand cmd = dbConnection.CreateCommand())
                    {
                        cmd.CommandText = $"CREATE SEQUENCE {SchemaPrefix}{SequenceName} START WITH 1 INCREMENT BY {Increment}";
                        cmd.ExecuteNonQuery();
                        Logger.Info($"Sequence {SchemaPrefix}{SequenceName} created");
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
                    command.CommandText = $"SELECT {SchemaPrefix}{SequenceName}.NEXTVAL FROM dual";
                    nextValue = Convert.ToInt32(command.ExecuteScalar());
                    Logger.Debug($"{SchemaPrefix}{SequenceName} served {nextValue} as next value");
                }

                return nextValue;
            }
        }

        public abstract int Increment { get; }
    }
}