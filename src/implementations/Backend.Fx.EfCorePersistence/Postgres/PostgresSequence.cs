namespace Backend.Fx.EfCorePersistence.Postgres
{
    using System;
    using System.Data;
    using Logging;
    
    public abstract class PostgresSequence : ISequence
    {
        private static readonly ILogger Logger = LogManager.Create<PostgresSequence>();

        public void EnsureSequence(IDbConnection dbConnection)
        {
            Logger.Info($"Ensuring existence of postgres sequence {SequenceName}");

            if (dbConnection.State == ConnectionState.Closed)
            {
                dbConnection.Open();
            }
            bool sequenceExists;
            using (IDbCommand command = dbConnection.CreateCommand())
            {
                command.CommandText = $"SELECT count(*) FROM information_schema.sequences WHERE sequence_name = '{SequenceName}'";
                sequenceExists = (long)command.ExecuteScalar() == 1L;
            }
            if (sequenceExists)
            {
                Logger.Info($"Sequence {SequenceName} exists");
            }
            else
            {
                Logger.Info($"Sequence {SequenceName} does not exist yet and will be created now");
                using (var cmd = dbConnection.CreateCommand())
                {
                    cmd.CommandText = $"CREATE SEQUENCE {SequenceName} START WITH 1 INCREMENT BY {Increment}";
                    cmd.ExecuteNonQuery();
                    Logger.Info($"Sequence {SequenceName} created");
                }
            }
        }

        public int GetNextValue(IDbConnection dbConnection)
        {
            if (dbConnection.State == ConnectionState.Closed)
            {
                dbConnection.Open();
            }
            int nextValue;
            using (IDbCommand command = dbConnection.CreateCommand())
            {
                command.CommandText = $"SELECT nextval('{SequenceName}');";
                nextValue = Convert.ToInt32(command.ExecuteScalar());
                Logger.Debug($"{SequenceName} served {nextValue} as next value");
            }
            return nextValue;
        }

        public abstract int Increment { get; }
        protected abstract string SequenceName { get; }
    }
}
