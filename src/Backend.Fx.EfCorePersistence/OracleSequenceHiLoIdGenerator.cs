namespace Backend.Fx.EfCorePersistence
{
    using System;
    using System.Data.Common;
    using Backend.Fx.Logging;
    using Backend.Fx.Patterns.IdGeneration;
    using Microsoft.EntityFrameworkCore;

    public abstract class OracleSequenceHiLoIdGenerator : HiLoIdGenerator
    {
        private static readonly ILogger Logger = LogManager.Create<OracleSequenceHiLoIdGenerator>();
        private readonly DbContextOptions dbContextOptions;
        private readonly string sequenceName;

        protected OracleSequenceHiLoIdGenerator(DbContextOptions dbContextOptions, string sequenceName)
        {
            this.dbContextOptions = dbContextOptions;
            this.sequenceName = sequenceName.ToUpperInvariant();
        }

        protected override int GetNextBlockStart()
        {
            using (DbContext dbContext = new DbContext(dbContextOptions))
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

        public void EnsureSqlSequenceExistence()
        {
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
                    if (!sequenceExists)
                    {
                        using (DbCommand command = dbConnection.CreateCommand())
                        {
                            command.CommandText = $"CREATE SEQUENCE {sequenceName} START WITH 1 INCREMENT BY {Increment}";
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
