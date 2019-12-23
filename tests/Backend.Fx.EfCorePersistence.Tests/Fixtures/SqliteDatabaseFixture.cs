using System.Data.Common;
using System.IO;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
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

        protected override DbContextOptionsBuilder<TestDbContext> GetDbContextOptionsBuilder(DbConnection connection)
        {
            return new DbContextOptionsBuilder<TestDbContext>().UseSqlite(connection);
        }

        public override DbSession UseDbSession()
        {
            var sqliteConnection = new SqliteConnection(_connectionString);
            return new DbSession(sqliteConnection, GetDbContextOptionsBuilder(sqliteConnection));
        }
    }
}