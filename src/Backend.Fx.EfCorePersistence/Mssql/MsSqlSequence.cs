namespace Backend.Fx.EfCorePersistence.Mssql
{
    using System;
    using System.Data.Common;
    using Logging;
    using Microsoft.EntityFrameworkCore;

    public abstract class MsSqlSequence : ISequence
    {
        private static readonly ILogger Logger = LogManager.Create<MsSqlSequence>();

        public void EnsureSequence(DbContext dbContext)
        {
            Logger.Info($"Ensuring existence of mssql sequence {SequenceName}");
            using (var dbConnection = dbContext.Database.GetDbConnection())
            {
                dbConnection.Open();

                bool sequenceExists;
                using (var cmd = dbConnection.CreateCommand())
                {
                    cmd.CommandText = $"SELECT count(*) FROM sys.sequences WHERE name = '{SequenceName}'";
                    sequenceExists = (int)cmd.ExecuteScalar() == 1;
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
        }

        public int GetNextValue(DbContext dbContext)
        {
            using (DbConnection dbConnection = dbContext.Database.GetDbConnection())
            {
                dbConnection.Open();
                int nextValue;
                using (DbCommand selectNextValCommand = dbConnection.CreateCommand())
                {
                    selectNextValCommand.CommandText = $"SELECT next value FOR {SequenceName}";
                    nextValue = Convert.ToInt32(selectNextValCommand.ExecuteScalar());
                    Logger.Debug($"{SequenceName} served {nextValue} as next value");
                }
                return nextValue;
            }
        }

        public abstract int Increment { get; }
        protected abstract string SequenceName { get; }
    }
}
