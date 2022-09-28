using System;
using System.Data;
using Backend.Fx.Features.IdGeneration;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Features.Persistence.AdoNet
{
    [PublicAPI]
    public abstract class OracleSequence : ISequence
    {
        private static readonly ILogger Logger = Log.Create<OracleSequence>();
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly int _startWith;

        protected OracleSequence(IDbConnectionFactory dbConnectionFactory, int startWith = 1)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _startWith = startWith;
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
            Logger.LogInformation("Ensuring existence of oracle sequence {SchemaPrefix}.{SequenceName}", SchemaPrefix, SequenceName);

            using IDbConnection dbConnection = _dbConnectionFactory.Create();
            dbConnection.Open();
            bool sequenceExists;
            using (IDbCommand command = dbConnection.CreateCommand())
            {
                command.CommandText = $"SELECT count(*) FROM user_sequences WHERE sequence_name = '{SequenceName}'";
                sequenceExists = (decimal)command.ExecuteScalar() == 1;
            }

            if (sequenceExists)
            {
                Logger.LogInformation("Oracle sequence {SchemaPrefix}.{SequenceName} exists", SchemaPrefix, SequenceName);
            }
            else
            {
                Logger.LogInformation("Oracle sequence {SchemaPrefix}.{SequenceName} does not exist yet and will be created now",
                    SchemaPrefix,
                    SequenceName);
                using IDbCommand cmd = dbConnection.CreateCommand();
                cmd.CommandText = $"CREATE SEQUENCE {SchemaPrefix}{SequenceName} START WITH {_startWith} INCREMENT BY {Increment}";
                cmd.ExecuteNonQuery();
                Logger.LogInformation("Oracle sequence {SchemaPrefix}.{SequenceName} created", SchemaPrefix, SequenceName);
            }
        }

        public int GetNextValue()
        {
            using IDbConnection dbConnection = _dbConnectionFactory.Create();
            dbConnection.Open();

            int nextValue;
            using IDbCommand command = dbConnection.CreateCommand();
            command.CommandText = $"SELECT {SchemaPrefix}{SequenceName}.NEXTVAL FROM dual";
            nextValue = Convert.ToInt32(command.ExecuteScalar());
            Logger.LogDebug("Oracle sequence {SchemaPrefix}.{SequenceName} served {NextValue} as next value",
                SchemaPrefix,
                SequenceName,
                nextValue);

            return nextValue;
        }

        public abstract int Increment { get; }
    }
}