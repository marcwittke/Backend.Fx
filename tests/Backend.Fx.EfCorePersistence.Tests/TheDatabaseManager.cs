using System.Data;
using System.Runtime.InteropServices.ComTypes;
using Backend.Fx.EfCorePersistence.Bootstrapping;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
using Backend.Fx.Patterns.DependencyInjection;
using FakeItEasy;
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
        private readonly IDatabaseBootstrapper _sut;
        private readonly DbContextOptions<TestDbContext> _dbContextOptions;
        private readonly string _dbFilePath;

        public TheDatabaseManager()
        {
            _dbFilePath = Path.GetTempFileName();
            string connectionString = "Data Source=" + _dbFilePath;
            _dbContextOptions = new DbContextOptionsBuilder<TestDbContext>().UseSqlite(connectionString).Options;

            TestCompositionRoot testCompRoot = new TestCompositionRoot();
            testCompRoot.Register<TestDbContext>(() => new TestDbContext(_dbContextOptions));
            testCompRoot.RegisterCollection<ISequence>(typeof(TheDatabaseManager).Assembly);
            testCompRoot.RegisterCollection<IFullTextSearchIndex>(typeof(TheDatabaseManager).Assembly);
            testCompRoot.Register<IDbConnection>(()=>new SqliteConnection(connectionString));
            _sut = new EfCreationDatabaseBootstrapper<TestDbContext>(testCompRoot);
        }

        [Fact]
        public void CreatesDatabase()
        {
            Assert.Throws<SqliteException>(() => new TestDbContext(_dbContextOptions).Tenants.ToArray());
            _sut.EnsureDatabaseExistence();
            Assert.Empty(new TestDbContext(_dbContextOptions).Tenants);
        }
    }
}
