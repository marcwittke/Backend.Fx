using System;
using System.Threading.Tasks;
using Backend.Fx.EfCore5Persistence.Tests.DummyImpl.Domain;
using Backend.Fx.EfCore5Persistence.Tests.DummyImpl.Persistence;
using Backend.Fx.EfCore5Persistence.Tests.Fixtures;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Extensions;
using Backend.Fx.Patterns.Authorization;
using Backend.Fx.TestUtil;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.EfCore5Persistence.Tests
{
    public class TheRepositoryOfPlainAggregate: TestWithLogging
    {
        public TheRepositoryOfPlainAggregate(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            //_fixture = new SqlServerDatabaseFixture();
            _fixture = new SqliteDatabaseFixture();
            _fixture.CreateDatabase();
        }

        private static int _nextTenantId = 12312;
        private readonly int _tenantId = _nextTenantId++;
        private readonly DatabaseFixture _fixture;

        [Fact]
        public void CanCreate()
        {
            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                var repo = new EfRepository<Blogger>(dbSession.DbContext, new BloggerMapping(), CurrentTenantIdHolder.Create(_tenantId), new AllowAll<Blogger>());
                repo.Add(new Blogger(345, "Metulsky", "Bratislav"));
            }

            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                var count = dbSession.DbConnection.ExecuteScalar<int>("SELECT Count(*) FROM Bloggers");
                Assert.Equal(1, count);

                count = dbSession.DbConnection.ExecuteScalar<int>(
                    $"SELECT Count(*) FROM Bloggers WHERE Id=345");
                Assert.Equal(1, count);
            }
        }
        
       
        [Fact]
        public void CanDelete()
        {
            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                dbSession.DbConnection.ExecuteNonQuery(
                    "INSERT INTO Bloggers (Id, TenantId, CreatedOn, CreatedBy, FirstName, LastName, Bio) " +
                    $"VALUES (555, {_tenantId}, '2012-05-12 23:12:09', 'the test', 'Bratislav', 'Metulsky', 'whatever')");
            }

            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                var repo = new EfRepository<Blogger>(dbSession.DbContext, new BloggerMapping(), CurrentTenantIdHolder.Create(_tenantId), new AllowAll<Blogger>());
                Blogger bratislavMetulsky = repo.Single(555);
                repo.Delete(bratislavMetulsky);
            }

            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                var count = dbSession.DbConnection.ExecuteScalar<int>("SELECT Count(*) FROM Bloggers WHERE Id = 555");
                Assert.Equal(0, count);
            }
        }
        
       

        [Fact]
        public void CanRead()
        {
            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                dbSession.DbConnection.ExecuteNonQuery(
                    "INSERT INTO Bloggers (Id, TenantId, CreatedOn, CreatedBy, FirstName, LastName, Bio) " +
                    $"VALUES (444, {_tenantId}, '2012-05-12 23:12:09', 'the test', 'Bratislav', 'Metulsky', 'whatever')");

                {
                    var repo = new EfRepository<Blogger>(dbSession.DbContext, new BloggerMapping(), CurrentTenantIdHolder.Create(_tenantId), new AllowAll<Blogger>());
                    
                    bool any = repo.Any();
                    Assert.True(any);
                    
                    Blogger[] all = repo.GetAll();
                    Assert.NotEmpty(all);
                    
                    Blogger bratislavMetulsky = repo.Single(444);
                    Assert.Equal(_tenantId, bratislavMetulsky.TenantId);
                    Assert.Equal("the test", bratislavMetulsky.CreatedBy);
                    Assert.Equal(new DateTime(2012, 05, 12, 23, 12, 09), bratislavMetulsky.CreatedOn);
                    Assert.Equal("Bratislav", bratislavMetulsky.FirstName);
                    Assert.Equal("Metulsky", bratislavMetulsky.LastName);
                    Assert.Equal("whatever", bratislavMetulsky.Bio);
                    
                    bratislavMetulsky = repo.SingleOrDefault(444);
                    Assert.NotNull(bratislavMetulsky);
                    Assert.Equal(_tenantId, bratislavMetulsky.TenantId);
                    Assert.Equal("the test", bratislavMetulsky.CreatedBy);
                    Assert.Equal(new DateTime(2012, 05, 12, 23, 12, 09), bratislavMetulsky.CreatedOn);
                    Assert.Equal("Bratislav", bratislavMetulsky.FirstName);
                    Assert.Equal("Metulsky", bratislavMetulsky.LastName);
                    Assert.Equal("whatever", bratislavMetulsky.Bio);
                }
            }
        }
        
        
        [Fact]
        public async Task CanReadAsync()
        {
            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                dbSession.DbConnection.ExecuteNonQuery(
                    "INSERT INTO Bloggers (Id, TenantId, CreatedOn, CreatedBy, FirstName, LastName, Bio) " +
                    $"VALUES (555, {_tenantId}, '2012-05-12 23:12:09', 'the test', 'Bratislav', 'Metulsky', 'whatever')");

                {
                    var repo = new EfRepository<Blogger>(dbSession.DbContext, new BloggerMapping(), CurrentTenantIdHolder.Create(_tenantId), new AllowAll<Blogger>());
                    
                    bool any = await repo.AnyAsync();
                    Assert.True(any);
                    
                    Blogger[] all = await repo.GetAllAsync();
                    Assert.NotEmpty(all);
                    
                    Blogger bratislavMetulsky = await repo.SingleAsync(555);
                    Assert.Equal(_tenantId, bratislavMetulsky.TenantId);
                    Assert.Equal("the test", bratislavMetulsky.CreatedBy);
                    Assert.Equal(new DateTime(2012, 05, 12, 23, 12, 09), bratislavMetulsky.CreatedOn);
                    Assert.Equal("Bratislav", bratislavMetulsky.FirstName);
                    Assert.Equal("Metulsky", bratislavMetulsky.LastName);
                    Assert.Equal("whatever", bratislavMetulsky.Bio);
                    
                    bratislavMetulsky = await repo.SingleOrDefaultAsync(555);
                    Assert.NotNull(bratislavMetulsky);
                    Assert.Equal(_tenantId, bratislavMetulsky.TenantId);
                    Assert.Equal("the test", bratislavMetulsky.CreatedBy);
                    Assert.Equal(new DateTime(2012, 05, 12, 23, 12, 09), bratislavMetulsky.CreatedOn);
                    Assert.Equal("Bratislav", bratislavMetulsky.FirstName);
                    Assert.Equal("Metulsky", bratislavMetulsky.LastName);
                    Assert.Equal("whatever", bratislavMetulsky.Bio);
                }
            }
        }

        [Fact]
        public void CanUpdate()
        {
            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                dbSession.DbConnection.ExecuteNonQuery(
                    "INSERT INTO Bloggers (Id, TenantId, CreatedOn, CreatedBy, FirstName, LastName, Bio) " +
                    $"VALUES (456, {_tenantId}, '2012-05-12 23:12:09', 'the test', 'Bratislav', 'Metulsky', 'whatever')");
            }

            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                var repo = new EfRepository<Blogger>(dbSession.DbContext, new BloggerMapping(),
                                                     CurrentTenantIdHolder.Create(_tenantId), new AllowAll<Blogger>());
                Blogger bratislavMetulsky = repo.Single(456);
                bratislavMetulsky.FirstName = "Johnny";
                bratislavMetulsky.LastName = "Flash";
                bratislavMetulsky.Bio = "Der lustige Clown";
            }

            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                var count = dbSession.DbConnection.ExecuteScalar<int>(
                    $"SELECT Count(*) FROM Bloggers WHERE FirstName = 'Johnny' AND LastName = 'Flash' AND TenantId = '{_tenantId}'");
                Assert.Equal(1, count);
            }

            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                var repo = new EfRepository<Blogger>(dbSession.DbContext, new BloggerMapping(), CurrentTenantIdHolder.Create(_tenantId), new AllowAll<Blogger>());
                Blogger johnnyFlash = repo.Single(456);
                Assert.Equal(DateTime.UtcNow, johnnyFlash.ChangedOn, new TolerantDateTimeComparer(TimeSpan.FromMilliseconds(5000)));
                Assert.Equal(new SystemIdentity().Name, johnnyFlash.ChangedBy);
                Assert.Equal("Johnny", johnnyFlash.FirstName);
                Assert.Equal("Flash", johnnyFlash.LastName);
            }
        }
    }
}