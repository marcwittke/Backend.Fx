namespace Backend.Fx.EfCorePersistence.Mssql
{
    using System;
    using Logging;
    using Microsoft.EntityFrameworkCore;

    public abstract class MsSqlSequenceHiLoIdGenerator<TDbContext> : SequenceHiLoIdGenerator where TDbContext : DbContext
    {
        private readonly DbContextOptions<TDbContext> dbContextOptions;
        private static readonly ILogger Logger = LogManager.Create<MsSqlSequenceHiLoIdGenerator<TDbContext>>();
        private readonly string sequenceName;

        protected MsSqlSequenceHiLoIdGenerator(DbContextOptions<TDbContext> dbContextOptions, string sequenceName)
        {
            this.dbContextOptions = dbContextOptions;
            this.sequenceName = sequenceName;
        }

        protected override int GetNextBlockStart()
        {
            using (DbContext dbContext = dbContextOptions.CreateDbContext())
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

        public override void EnsureSqlSequenceExistence()
        {
            Logger.Info($"Ensuring existence of ms sql sequence {sequenceName}");
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
        }
    }
}
