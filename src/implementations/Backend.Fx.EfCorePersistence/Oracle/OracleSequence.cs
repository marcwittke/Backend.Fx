namespace Backend.Fx.EfCorePersistence.Oracle
{
    using System;
    using System.Data;
    using Logging;

    public abstract class OracleSequence : ISequence
    {
        private static readonly ILogger Logger = LogManager.Create<OracleSequence>();

        public void EnsureSequence(IDbConnection dbConnection)
        {
            Logger.Info($"Ensuring existence of oracle sequence {SchemaPrefix}{SequenceName}");

            if (dbConnection.State == ConnectionState.Closed)
            {
                dbConnection.Open();
            }

            bool sequenceExists;
            using (IDbCommand command = dbConnection.CreateCommand())
            {
                command.CommandText = $"SELECT count(*) FROM user_sequences WHERE sequence_name = '{SequenceName}'";
                sequenceExists = (decimal)command.ExecuteScalar() == 1;
            }
            if (sequenceExists)
            {
                Logger.Info($"Sequence {SchemaPrefix}{SequenceName} exists");
            }
            else
            {
                Logger.Info($"Sequence {SchemaPrefix}{SequenceName} does not exist yet and will be created now");
                using (var cmd = dbConnection.CreateCommand())
                {
                    cmd.CommandText = $"CREATE SEQUENCE {SchemaPrefix}{SequenceName} START WITH 1 INCREMENT BY {Increment}";
                    cmd.ExecuteNonQuery();
                    Logger.Info($"Sequence {SchemaPrefix}{SequenceName} created");
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
                command.CommandText = $"SELECT {SchemaPrefix}{SequenceName}.NEXTVAL FROM dual";
                nextValue = Convert.ToInt32(command.ExecuteScalar());
                Logger.Debug($"{SchemaPrefix}{SequenceName} served {nextValue} as next value");
            }
            return nextValue;
        }

        public abstract int Increment { get; }
        protected abstract string SequenceName { get; }
        protected abstract string SchemaName { get; }

        private string SchemaPrefix
        {
            get
            {
                if (string.IsNullOrEmpty(SchemaName))
                {
                    return string.Empty;
                }

                return SchemaName + ".";
            }
        }
    }
}
