namespace Backend.Fx.EfCorePersistence
{
    using Logging;
    using Microsoft.EntityFrameworkCore;
    using Patterns.IdGeneration;

    public abstract class SqlSequenceHiLoIdGenerator : HiLoIdGenerator
    {
        private readonly DbContext dbContext;
        private static readonly ILogger Logger = LogManager.Create<SqlSequenceHiLoIdGenerator>();
        private readonly string sequenceName;

        protected SqlSequenceHiLoIdGenerator(DbContext dbContext, string sequenceName)
        {
            this.dbContext = dbContext;
            this.sequenceName = sequenceName;
        }

        protected override int GetNextBlockStart()
        {
            using (var dbConnection = dbContext.Database.GetDbConnection())
            {
                int nextValFromSequence;
                dbConnection.Open();
                using (var selectNextValCommand = dbConnection.CreateCommand())
                {
                    selectNextValCommand.CommandText = "SELECT next value FOR {sequenceName}";
                    nextValFromSequence = (int)selectNextValCommand.ExecuteScalar();
                    Logger.Debug("{0} served {1} as next value", sequenceName, nextValFromSequence);
                }
                return nextValFromSequence;
            }
        }
        
        public void EnsureSqlSequenceExistence()
        {
            using (var dbConnection = dbContext.Database.GetDbConnection())
            {
                dbConnection.Open();

                bool sequenceExists;
                using (var cmd = dbConnection.CreateCommand())
                {
                    cmd.CommandText = $"SELECT count(*) FROM sys.sequences WHERE name = '{sequenceName}'";
                    sequenceExists = (int) cmd.ExecuteScalar() == 1;
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
