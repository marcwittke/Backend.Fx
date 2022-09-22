using System.Data;
using System.IO;
using Backend.Fx.EfCore5Persistence.Tests.SampleApp.Persistence;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Extensions.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCore5Persistence.Tests.Fixtures
{
    public class SqliteDatabaseFixture : DatabaseFixture
    {
        private readonly string _connectionString = "Data Source=" + Path.GetTempFileName();

        protected override DbContextOptions<SampleAppDbContext> GetDbContextOptionsForDbCreation()
        {
            return new DbContextOptionsBuilder<SampleAppDbContext>().UseSqlite(_connectionString).Options;
        }

        public override DbContextOptionsBuilder<SampleAppDbContext> GetDbContextOptionsBuilder(IDbConnection connection)
        {
            return new DbContextOptionsBuilder<SampleAppDbContext>().UseSqlite((SqliteConnection) connection);
        }

        public override DbConnectionOperationDecorator UseOperation()
        {
            var sqliteConnection = new SqliteConnection(_connectionString);
            IOperation operation = new Operation();
            operation = new DbTransactionOperationDecorator(sqliteConnection, operation);
            return new DbConnectionOperationDecorator(sqliteConnection, operation);
        }
    }
}