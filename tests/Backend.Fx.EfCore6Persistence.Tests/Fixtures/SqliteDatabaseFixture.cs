using System.Data;
using System.IO;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence.Tests.Fixtures
{
    public class SqliteDatabaseFixture : DatabaseFixture
    {
        private readonly string _connectionString = "Data Source=" + Path.GetTempFileName();

        protected override DbContextOptions<TestDbContext> GetDbContextOptionsForDbCreation()
        {
            return new DbContextOptionsBuilder<TestDbContext>().UseSqlite(_connectionString).Options;
        }

        public override DbContextOptionsBuilder<TestDbContext> GetDbContextOptionsBuilder(IDbConnection connection)
        {
            return new DbContextOptionsBuilder<TestDbContext>().UseSqlite((SqliteConnection) connection);
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