using System.Data.Common;
using System.IO;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence.Tests
{
    public class TestWithSqliteFileDbContext : TestWithDbContext
    {
        public override DbContextOptions<TestDbContext> DbContextOptions()
        {
            Connection.Open();
            return new DbContextOptionsBuilder<TestDbContext>().UseSqlite((DbConnection)Connection).Options;
        }

        public override TestDbContext UseDbContext()
        {
            DbContextOptions<TestDbContext> options = new DbContextOptionsBuilder<TestDbContext>().UseSqlite((DbConnection)Connection).Options;
            return new TestDbContext(options);
        }

        public TestWithSqliteFileDbContext() : base(new SqliteConnection("Data Source=" + Path.GetTempFileName()))
        {
        }
    }
}