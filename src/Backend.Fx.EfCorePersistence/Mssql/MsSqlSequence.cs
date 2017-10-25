namespace Backend.Fx.EfCorePersistence.Mssql
{
    using System;
    using System.Data.Common;
    using Logging;
    using Microsoft.EntityFrameworkCore;

    public class MsSqlSequence : ISequence
    {
        private readonly string sequenceName;
        private static readonly ILogger Logger = LogManager.Create<MsSqlSequence>();

        public MsSqlSequence(string sequenceName, int increment)
        {
            this.sequenceName = sequenceName;
            Increment = increment;
        }

        public void EnsureSequence(DbContext dbContext)
        {
            Logger.Info($"Ensuring existence of oracle sequence {sequenceName}");
            using (var dbConnection = dbContext.Database.GetDbConnection())
            {
                dbConnection.Open();

                bool sequenceExists;
                using (var cmd = dbConnection.CreateCommand())
                {
                    cmd.CommandText = $"SELECT count(*) FROM sys.sequences WHERE name = '{sequenceName}'";
                    sequenceExists = (int)cmd.ExecuteScalar() == 1;
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
                using (DbCommand selectNextValCommand = dbConnection.CreateCommand())
                {
                    selectNextValCommand.CommandText = $"SELECT next value FOR {sequenceName}";
                    nextValue = Convert.ToInt32(selectNextValCommand.ExecuteScalar());
                    Logger.Debug($"{sequenceName} served {nextValue} as next value");
                }
                return nextValue;
            }
        }

        public int Increment { get; }
    }
}
