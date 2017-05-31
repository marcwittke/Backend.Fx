namespace Backend.Fx.EfCorePersistence.Tests
{
    using System.IO;
    using System.Linq;
    using DummyImpl;
    using Environment.Persistence;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using NLogLogging;
    using Xunit;

    public class TheDatabaseManager : IClassFixture<NLogLoggingFixture>
    {
        private readonly IDatabaseManager sut;
        private readonly DbContextOptions dbContextOptions;
        private readonly string dbFilePath;

        public TheDatabaseManager()
        {
            dbFilePath = Path.GetTempFileName();
            dbContextOptions = new DbContextOptionsBuilder().UseSqlite("Data Source=" + dbFilePath).Options;
            sut = new DatabaseManager<TestDbContext>(() => new TestDbContext(dbContextOptions));
        }

        [Fact]
        public void CreatesDatabase()
        {
            Assert.Throws<SqliteException>(()=>new TestDbContext(dbContextOptions).Tenants.ToArray());
            Assert.False(sut.DatabaseExists);
            sut.EnsureDatabaseExistence();
            Assert.True(sut.DatabaseExists);
            Assert.Empty(new TestDbContext(dbContextOptions).Tenants);
        }

        [Fact]
        public void DeletesDatabase()
        {
            SqliteConnection connection = new SqliteConnection("Data Source=" + dbFilePath);
            connection.Open();
            connection.Close();
            Assert.True(File.Exists(dbFilePath));

            sut.DeleteDatabase();
            Assert.False(sut.DatabaseExists);
            Assert.False(File.Exists(dbFilePath));
        }
    }
}
