namespace Backend.Fx.EfCorePersistence.Oracle
{
    using System;
    using System.Data.Common;
    using Logging;
    using Microsoft.EntityFrameworkCore;
    using Patterns.IdGeneration;

    public abstract class OracleSequenceHiLoIdGenerator<TDbContext> : HiLoIdGenerator where TDbContext : DbContext
    {
        private static readonly ILogger Logger = LogManager.Create<OracleSequenceHiLoIdGenerator<TDbContext>>();
        private readonly DbContextOptions<TDbContext> dbContextOptions;
        private readonly string sequenceName;

        protected OracleSequenceHiLoIdGenerator(DbContextOptions<TDbContext> dbContextOptions, string sequenceName)
        {
            this.dbContextOptions = dbContextOptions;
            this.sequenceName = sequenceName.ToUpperInvariant();
        }

        protected override int GetNextBlockStart()
        {
            using (DbContext dbContext = dbContextOptions.CreateDbContext())
            {
                using (DbConnection dbConnection = dbContext.Database.GetDbConnection())
                {
                    dbConnection.Open();
                    int nextBlockStart;
                    using (DbCommand command = dbConnection.CreateCommand())
                    {
                        command.CommandText = $"SELECT {sequenceName}.NEXTVAL FROM dual";
                        nextBlockStart = Convert.ToInt32(command.ExecuteScalar());
                        Logger.Debug($"{sequenceName} served {nextBlockStart} as next value");
                    }
                    return nextBlockStart;
                }
            }
        }

        public void Initialize()
        {
            Logger.Info($"Ensuring existence of oracle sequence {sequenceName}");
            using (DbContext dbContext = new DbContext(dbContextOptions))
            {
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
        }
    }
}
