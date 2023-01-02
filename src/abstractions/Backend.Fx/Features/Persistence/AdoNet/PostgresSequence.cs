using System;
using System.Data;
using Backend.Fx.Features.IdGeneration;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Features.Persistence.AdoNet
{
    public abstract class PostgresSequence<TId> : ISequence<TId> 
    {
        private readonly ILogger _logger = Log.Create<PostgresSequence<TId>>();
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly int _startWith;

        protected PostgresSequence(IDbConnectionFactory dbConnectionFactory, int startWith = 1)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _startWith = startWith;
        }

        protected abstract string SequenceName { get; }
        protected abstract string SchemaName { get; }

        public void EnsureSequence()
        {
            _logger.LogInformation("Ensuring existence of postgres sequence {SchemaName}.{SequenceName}", SchemaName, SequenceName);

            using IDbConnection dbConnection = _dbConnectionFactory.Create();
            dbConnection.Open();
            bool sequenceExists;
            using (IDbCommand command = dbConnection.CreateCommand())
            {
                command.CommandText = $"SELECT count(*) FROM information_schema.sequences WHERE sequence_name = '{SequenceName}' AND sequence_schema = '{SchemaName}'";
                sequenceExists = (long) command.ExecuteScalar() == 1L;
            }

            if (sequenceExists)
            {
                _logger.LogInformation("Sequence {SchemaName}.{SequenceName} exists", SchemaName, SequenceName);
            }
            else
            {
                _logger.LogInformation(
                    "Sequence {SchemaName}.{SequenceName} does not exist yet and will be created now",
                    SchemaName,
                    SequenceName);
                using IDbCommand cmd = dbConnection.CreateCommand();
                cmd.CommandText = $"CREATE SEQUENCE {SchemaName}.{SequenceName} START WITH {_startWith} INCREMENT BY {Increment}";
                cmd.ExecuteNonQuery();
                _logger.LogInformation("Sequence {SchemaName}.{SequenceName} created", SchemaName, SequenceName);
            }
        }

        public TId GetNextValue()
        {
            using IDbConnection dbConnection = _dbConnectionFactory.Create();
            dbConnection.Open();

            using IDbCommand command = dbConnection.CreateCommand();
            command.CommandText = $"SELECT nextval('{SchemaName}.{SequenceName}');";
            TId nextValue = ConvertNextValueFromSequence(command.ExecuteScalar());
            _logger.LogDebug("{SchemaName}.{SequenceName} served {2} as next value", SchemaName, SequenceName, nextValue);

            return nextValue;
        }

        public abstract TId Increment { get; }
        
        protected abstract TId ConvertNextValueFromSequence(object valueFromSequence);
    }
    
    [PublicAPI]
    public abstract class PostgresIntSequence : PostgresSequence<int>
    {
        protected PostgresIntSequence(IDbConnectionFactory dbConnectionFactory, int startWith = 1) 
            : base(dbConnectionFactory, startWith)
        {
        }

        protected override int ConvertNextValueFromSequence(object valueFromSequence)
        {
            return Convert.ToInt32(valueFromSequence);
        }
    }
    
    [PublicAPI]
    public abstract class PostgresLongSequence : PostgresSequence<long>
    {
        protected PostgresLongSequence(IDbConnectionFactory dbConnectionFactory, int startWith = 1) 
            : base(dbConnectionFactory, startWith)
        {
        }

        protected override long ConvertNextValueFromSequence(object valueFromSequence)
        {
            return Convert.ToInt64(valueFromSequence);
        }
    }
}