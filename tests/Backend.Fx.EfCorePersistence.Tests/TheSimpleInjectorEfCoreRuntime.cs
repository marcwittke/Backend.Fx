namespace Backend.Fx.EfCorePersistence.Tests
{
    using Microsoft.Data.Sqlite;
    using System.IO;
    using System.Linq;
    using Bootstrapping;
    using DummyImpl;
    using Environment.MultiTenancy;
    using Microsoft.EntityFrameworkCore;
    using NLogLogging;
    using Xunit;

    public class TheSimpleInjectorEfCoreRuntime : IClassFixture<NLogLoggingFixture>
    {
        private readonly SimpleInjectorRuntime sut;
        private readonly DbContextOptions dbContextOptions;
        private readonly string dbFilePath;

        public TheSimpleInjectorEfCoreRuntime()
        {
            dbFilePath = Path.GetTempFileName();
            dbContextOptions = new DbContextOptionsBuilder().UseSqlite("Data Source=" + dbFilePath).Options;
            sut = new TestRuntime(dbContextOptions);
        }

        [Fact]
        public void BootingCreatesDatabase()
        {
            sut.Boot();
            Assert.True(File.Exists(dbFilePath));
        }

        [Fact]
        public void BootingMigratesDatabase()
        {
            SqliteConnection connection = new SqliteConnection("Data Source=" + dbFilePath);
            connection.Open();
            connection.Close();
            Assert.True(File.Exists(dbFilePath));
            sut.Boot();

            using (var dbContext = new TestDbContext(dbContextOptions))
            {
                Assert.Empty(dbContext.Set<Tenant>().ToArray());
            }
        }

    }
}
