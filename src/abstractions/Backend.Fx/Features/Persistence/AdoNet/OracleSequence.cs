using System;
using System.Data;
using Backend.Fx.Features.IdGeneration;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Features.Persistence.AdoNet
{
    public abstract class OracleSequence<TId> : ISequence<TId> 
    {
        private readonly ILogger _logger = Log.Create<OracleSequence<TId>>();
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
            _logger.LogInformation("Ensuring existence of oracle sequence {SchemaPrefix}.{SequenceName}", SchemaPrefix, SequenceName);

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
                _logger.LogInformation("Oracle sequence {SchemaPrefix}.{SequenceName} exists", SchemaPrefix, SequenceName);
            }
            else
            {
                _logger.LogInformation("Oracle sequence {SchemaPrefix}.{SequenceName} does not exist yet and will be created now",
                    SchemaPrefix,
                    SequenceName);
                using IDbCommand cmd = dbConnection.CreateCommand();
                cmd.CommandText = $"CREATE SEQUENCE {SchemaPrefix}{SequenceName} START WITH {_startWith} INCREMENT BY {Increment}";
                cmd.ExecuteNonQuery();
                _logger.LogInformation("Oracle sequence {SchemaPrefix}.{SequenceName} created", SchemaPrefix, SequenceName);
            }
        }

        public TId GetNextValue()
        {
            using IDbConnection dbConnection = _dbConnectionFactory.Create();
            dbConnection.Open();

            using IDbCommand command = dbConnection.CreateCommand();
            command.CommandText = $"SELECT {SchemaPrefix}{SequenceName}.NEXTVAL FROM dual";
            TId nextValue = ConvertNextValueFromSequence(command.ExecuteScalar());
            _logger.LogDebug("Oracle sequence {SchemaPrefix}.{SequenceName} served {NextValue} as next value",
                SchemaPrefix,
                SequenceName,
                nextValue);

            return nextValue;
        }

        public abstract TId Increment { get; }
        
        protected abstract TId ConvertNextValueFromSequence(object valueFromSequence);
    }
    
    [PublicAPI]
    public abstract class OracleIntSequence : OracleSequence<int>
    {
        protected OracleIntSequence(IDbConnectionFactory dbConnectionFactory, int startWith = 1) 
            : base(dbConnectionFactory, startWith)
        {
        }

        protected override int ConvertNextValueFromSequence(object valueFromSequence)
        {
            return Convert.ToInt32(valueFromSequence);
        }
    }
    
    [PublicAPI]
    public abstract class OracleLongSequence : OracleSequence<long>
    {
        protected OracleLongSequence(IDbConnectionFactory dbConnectionFactory, int startWith = 1) 
            : base(dbConnectionFactory, startWith)
        {
        }

        protected override long ConvertNextValueFromSequence(object valueFromSequence)
        {
            return Convert.ToInt64(valueFromSequence);
        }
    }
}