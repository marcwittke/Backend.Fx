namespace Backend.Fx.EfCorePersistence
{
    using System;
    using Logging;
    using Microsoft.EntityFrameworkCore;
    using Patterns.IdGeneration;

    public abstract class SqlSequenceHiLoIdGenerator : HiLoIdGenerator
    {
        private readonly DbContextOptions dbContextOptions;
        private static readonly ILogger Logger = LogManager.Create<SqlSequenceHiLoIdGenerator>();
        private readonly string sequenceName;

        protected SqlSequenceHiLoIdGenerator(DbContextOptions dbContextOptions, string sequenceName)
        {
            this.dbContextOptions = dbContextOptions;
            this.sequenceName = sequenceName;
        }

        protected override int GetNextBlockStart()
        {
            using (var dbContext = new DbContext(dbContextOptions))
            {
                using (var dbConnection = dbContext.Database.GetDbConnection())
                {
                    int nextValFromSequence;
                    dbConnection.Open();
                    using (var selectNextValCommand = dbConnection.CreateCommand())
                    {
                        selectNextValCommand.CommandText = $"SELECT next value FOR {sequenceName}";
                        nextValFromSequence = Convert.ToInt32(selectNextValCommand.ExecuteScalar());
                        Logger.Debug("{0} served {1} as next value", sequenceName, nextValFromSequence);
                    }
                    return nextValFromSequence;
                }
            }
        }

        public void EnsureSqlSequenceExistence()
        {
            using (var dbContext = new DbContext(dbContextOptions))
            {
                using (var dbConnection = dbContext.Database.GetDbConnection())
                {
                    dbConnection.Open();

                    bool sequenceExists;
                    using (var cmd = dbConnection.CreateCommand())
                    {
                        cmd.CommandText = $"SELECT count(*) FROM sys.sequences WHERE name = '{sequenceName}'";
                        sequenceExists = (int)cmd.ExecuteScalar() == 1;
                    }

                    if (!sequenceExists)
                    {
                        using (var cmd = dbConnection.CreateCommand())
                        {
                            cmd.CommandText = $"CREATE SEQUENCE {sequenceName} START WITH 1 INCREMENT BY {Increment}";
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
