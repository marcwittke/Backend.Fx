namespace Backend.Fx.EfCorePersistence.Oracle
{
    using System;
    using System.Data.Common;
    using Logging;
    using Microsoft.EntityFrameworkCore;

    public class OracleSequence : ISequence
    {
        private readonly string sequenceName;
        private static readonly ILogger Logger = LogManager.Create<OracleSequence>();

        public OracleSequence(string sequenceName, int increment)
        {
            this.sequenceName = sequenceName.ToUpperInvariant();
            Increment = increment;
        }

        public void EnsureSequence(DbContext dbContext)
        {
            Logger.Info($"Ensuring existence of oracle sequence {sequenceName}");
            using (DbConnection dbConnection = dbContext.Database.GetDbConnection())
            {
                dbConnection.Open();
                bool sequenceExists;
                using (DbCommand command = dbConnection.CreateCommand())
                {
                    command.CommandText = $"SELECT count(*) FROM user_sequences WHERE sequence_name = '{sequenceName}'";
                    sequenceExists = (decimal)command.ExecuteScalar() == 1;
                }
                if (sequenceExists)
                {
                    Logger.Info($"Sequence {sequenceName} exists");
                }
                else
                {
                    Logger.Info($"Sequence {sequenceName} does not exist yet and will be created now");
                    using (var cmd = dbConnection.CreateCommand())
                    {
                        cmd.CommandText = $"CREATE SEQUENCE {sequenceName} START WITH 1 INCREMENT BY {Increment}";
                        cmd.ExecuteNonQuery();
                        Logger.Info($"Sequence {sequenceName} created");
                    }
                }
            }
        }

        public int GetNextValue(DbContext dbContext)
        {
            using (DbConnection dbConnection = dbContext.Database.GetDbConnection())
            {
                dbConnection.Open();
                int nextValue;
                using (DbCommand command = dbConnection.CreateCommand())
                {
                    command.CommandText = $"SELECT {sequenceName}.NEXTVAL FROM dual";
                    nextValue = Convert.ToInt32(command.ExecuteScalar());
                    Logger.Debug($"{sequenceName} served {nextValue} as next value");
                }
                return nextValue;
            }
        }

        public int Increment { get; }
    }
}
