using System.Data;
using System.IO;
using System.Linq;
using Backend.Fx.EfCorePersistence.Bootstrapping;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
using Backend.Fx.Environment.Persistence;
using FakeItEasy;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Backend.Fx.EfCorePersistence.Tests
{
    public class TheDatabaseManager
    {
        private readonly IDatabaseBootstrapper _sut;
        private readonly DbContextOptions<TestDbContext> _dbContextOptions;
        private readonly IDatabaseBootstrapperInstanceProvider _instanceProvider;
        private readonly ISequence _aSequence;
        private readonly IFullTextSearchIndex _aSearchIndex;

        public TheDatabaseManager()
        {
            var dbFilePath = Path.GetTempFileName();
            _dbContextOptions = new DbContextOptionsBuilder<TestDbContext>().UseSqlite("Data Source=" + dbFilePath).Options;

            _instanceProvider = A.Fake<IDatabaseBootstrapperInstanceProvider>();
            _aSequence = A.Fake<ISequence>();
            _aSearchIndex = A.Fake<IFullTextSearchIndex>();
            A.CallTo(() => _instanceProvider.GetAllSearchIndizes()).Returns(new[] {_aSearchIndex});
            A.CallTo(() => _instanceProvider.GetAllSequences()).Returns(new [] {_aSequence});

            _sut = new EfCreationDatabaseBootstrapper<TestDbContext>(new TestDbContext(_dbContextOptions), _instanceProvider);
        }

        [Fact]
        public void CreatesDatabase()
        {
            Assert.Throws<SqliteException>(() => new TestDbContext(_dbContextOptions).Tenants.ToArray());
            _sut.EnsureDatabaseExistence();
            Assert.Empty(new TestDbContext(_dbContextOptions).Tenants);

            A.CallTo(() => _instanceProvider.GetAllSequences()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _aSequence.EnsureSequence(A<IDbConnection>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => _instanceProvider.GetAllSearchIndizes()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _aSearchIndex.EnsureIndex(A<TestDbContext>._)).MustHaveHappenedOnceExactly();
        }
    }
}
