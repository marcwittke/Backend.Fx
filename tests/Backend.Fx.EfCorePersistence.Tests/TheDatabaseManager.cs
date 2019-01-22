using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
using Xunit;

namespace Backend.Fx.EfCorePersistence.Tests
{
    using System.IO;
    using System.Linq;
    using Environment.Persistence;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;

    public class TheDatabaseManager
    {
        private readonly IDatabaseManager _sut;
        private readonly DbContextOptions<TestDbContext> _dbContextOptions;
        private readonly string _dbFilePath;

        public TheDatabaseManager()
        {
            _dbFilePath = Path.GetTempFileName();
            _dbContextOptions = new DbContextOptionsBuilder<TestDbContext>().UseSqlite("Data Source=" + _dbFilePath).Options;
            _sut = new DatabaseManagerWithoutMigration<TestDbContext>(() => new TestDbContext(_dbContextOptions));
        }

        [Fact]
        public void CreatesDatabase()
        {
            Assert.Throws<SqliteException>(() => new TestDbContext(_dbContextOptions).Tenants.ToArray());
            _sut.EnsureDatabaseExistence();
            Assert.Empty(new TestDbContext(_dbContextOptions).Tenants);
        }

        [Fact]
        public void DeletesDatabase()
        {
            SqliteConnection connection = new SqliteConnection("Data Source=" + _dbFilePath);
            connection.Open();
            connection.Close();
            Assert.True(File.Exists(_dbFilePath));

            _sut.DeleteDatabase();
            Assert.False(File.Exists(_dbFilePath));
        }
    }
}
