namespace Backend.Fx.EfCorePersistence.Tests
{
    using System.IO;
    using System.Linq;
    using DummyImpl;
    using Environment.Persistence;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using Xunit;

    public class TheDatabaseManager
    {
        private readonly IDatabaseManager sut;
        private readonly DbContextOptions<TestDbContext> dbContextOptions;
        private readonly string dbFilePath;

        public TheDatabaseManager()
        {
            dbFilePath = Path.GetTempFileName();
            dbContextOptions = new DbContextOptionsBuilder<TestDbContext>().UseSqlite("Data Source=" + dbFilePath).Options;
            sut = new DatabaseManagerWithoutMigration<TestDbContext>(dbContextOptions);
        }

        [Fact]
        public void CreatesDatabase()
        {
            Assert.Throws<SqliteException>(()=>new TestDbContext(dbContextOptions).Tenants.ToArray());
            sut.EnsureDatabaseExistence();
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
            Assert.False(File.Exists(dbFilePath));
        }
    }
}
