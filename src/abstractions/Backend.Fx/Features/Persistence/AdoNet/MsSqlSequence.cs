using System;
using System.Data;
using Backend.Fx.Features.IdGeneration;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Features.Persistence.AdoNet
{
    [PublicAPI]
    public abstract class MsSqlSequence<TId> : ISequence<TId> 
    {
        private static readonly ILogger Logger = Log.Create<MsSqlSequence<TId>>();
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly int _startWith;

        protected MsSqlSequence(IDbConnectionFactory dbConnectionFactory, int startWith = 1)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _startWith = startWith;
        }

        protected abstract string SequenceName { get; }
        protected virtual string SchemaName { get; } = "dbo";

        public void EnsureSequence()
        {
            Logger.LogInformation("Ensuring existence of mssql sequence {SchemaName}.{SequenceName}", SchemaName, SequenceName);
            using IDbConnection dbConnection = _dbConnectionFactory.Create();
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
                using IDbCommand cmd = dbConnection.CreateCommand();
                cmd.CommandText = $"CREATE SEQUENCE [{SchemaName}].[{SequenceName}] START WITH {_startWith} INCREMENT BY {Increment}";
                cmd.ExecuteNonQuery();
                Logger.LogInformation("Sequence {SchemaName}.{SequenceName} created", SchemaName, SequenceName);
            }
        }

        public TId GetNextValue()
        {
            using IDbConnection dbConnection = _dbConnectionFactory.Create();
            dbConnection.Open();
            using IDbCommand selectNextValCommand = dbConnection.CreateCommand();
            selectNextValCommand.CommandText = $"SELECT next value FOR {SchemaName}.{SequenceName}";
            TId nextValue = ConvertNextValueFromSequence(selectNextValCommand.ExecuteScalar());
            Logger.LogDebug("{SchemaName}.{SequenceName} served {NextValue} as next value", SchemaName, SequenceName, nextValue);

            return nextValue;
        }

        public abstract TId Increment { get; }

        protected abstract TId ConvertNextValueFromSequence(object valueFromSequence);
    }
    
    [PublicAPI]
    public abstract class MsSqlIntSequence : MsSqlSequence<int>
    {
        protected MsSqlIntSequence(IDbConnectionFactory dbConnectionFactory, int startWith = 1) 
            : base(dbConnectionFactory, startWith)
        {
        }

        protected override int ConvertNextValueFromSequence(object valueFromSequence)
        {
            return Convert.ToInt32(valueFromSequence);
        }
    }
    
    [PublicAPI]
    public abstract class MsSqlLongSequence : MsSqlSequence<long>
    {
        protected MsSqlLongSequence(IDbConnectionFactory dbConnectionFactory, int startWith = 1) 
            : base(dbConnectionFactory, startWith)
        {
        }

        protected override long ConvertNextValueFromSequence(object valueFromSequence)
        {
            return Convert.ToInt64(valueFromSequence);
        }
    }
}