using System;
using System.Threading.Tasks;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain;
using Backend.Fx.EfCorePersistence.Tests.Fixtures;
using Backend.Fx.Extensions;
using Dapper;
using Xunit;

namespace Backend.Fx.EfCorePersistence.Tests
{
    public class TheRepositoryOfPlainAggregate : IClassFixture<PersistenceFixture>
    {
        public TheRepositoryOfPlainAggregate(PersistenceFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly PersistenceFixture _fixture;

        [Fact]
        public void CanCreate()
        {
            using (var scope = _fixture.BeginScope())
            {
                var repo = scope.GetRepository<Blogger>();
                repo.Add(new Blogger(345, "Metulsky", "Bratislav"));
            }

            using (var scope = _fixture.BeginScope())
            {
                var count = scope.DbConnection.ExecuteScalar<int>("SELECT Count(*) FROM Bloggers");
                Assert.Equal(1, count);

                count = scope.DbConnection.ExecuteScalar<int>($"SELECT Count(*) FROM Bloggers WHERE Id=345");
                Assert.Equal(1, count);
            }
        }


        [Fact]
        public void CanDelete()
        {
            using (var scope = _fixture.BeginScope())
            {
                scope.DbConnection.Execute(
                    "INSERT INTO Bloggers (Id, TenantId, CreatedOn, CreatedBy, FirstName, LastName, Bio) " +
                    $"VALUES (555, {scope.TenantId.Value}, '2012-05-12 23:12:09', 'the test', 'Bratislav', 'Metulsky', 'whatever')");
            }

            using (var scope = _fixture.BeginScope())
            {
                var repo = scope.GetRepository<Blogger>();
                Blogger bratislavMetulsky = repo.Single(555);
                repo.Delete(bratislavMetulsky);
            }

            using (var scope = _fixture.BeginScope())
            {
                var count = scope.DbConnection.ExecuteScalar<int>("SELECT Count(*) FROM Bloggers WHERE Id = 555");
                Assert.Equal(0, count);
            }
        }


        [Fact]
        public void CanRead()
        {
            using (var scope = _fixture.BeginScope())
            {
                scope.DbConnection.Execute(
                    "INSERT INTO Bloggers (Id, TenantId, CreatedOn, CreatedBy, FirstName, LastName, Bio) " +
                    $"VALUES (444, {scope.TenantId.Value}, '2012-05-12 23:12:09', 'the test', 'Bratislav', 'Metulsky', 'whatever')");

                {
                    var repo = scope.GetRepository<Blogger>();

                    bool any = repo.Any();
                    Assert.True(any);

                    Blogger[] all = repo.GetAll();
                    Assert.NotEmpty(all);

                    Blogger bratislavMetulsky = repo.Single(444);
                    Assert.Equal(scope.TenantId.Value, bratislavMetulsky.TenantId);
                    Assert.Equal("the test", bratislavMetulsky.CreatedBy);
                    Assert.Equal(new DateTime(2012, 05, 12, 23, 12, 09), bratislavMetulsky.CreatedOn);
                    Assert.Equal("Bratislav", bratislavMetulsky.FirstName);
                    Assert.Equal("Metulsky", bratislavMetulsky.LastName);
                    Assert.Equal("whatever", bratislavMetulsky.Bio);

                    bratislavMetulsky = repo.SingleOrDefault(444);
                    Assert.NotNull(bratislavMetulsky);
                    Assert.Equal(scope.TenantId.Value, bratislavMetulsky.TenantId);
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
            using (var scope = _fixture.BeginScope())
            {
                scope.DbConnection.Execute(
                    "INSERT INTO Bloggers (Id, TenantId, CreatedOn, CreatedBy, FirstName, LastName, Bio) " +
                    $"VALUES (555, {scope.TenantId.Value}, '2012-05-12 23:12:09', 'the test', 'Bratislav', 'Metulsky', 'whatever')");

                {
                    var repo = scope.GetAsyncRepository<Blogger>();

                    bool any = await repo.AnyAsync();
                    Assert.True(any);

                    Blogger[] all = await repo.GetAllAsync();
                    Assert.NotEmpty(all);

                    Blogger bratislavMetulsky = await repo.SingleAsync(555);
                    Assert.Equal(scope.TenantId.Value, bratislavMetulsky.TenantId);
                    Assert.Equal("the test", bratislavMetulsky.CreatedBy);
                    Assert.Equal(new DateTime(2012, 05, 12, 23, 12, 09), bratislavMetulsky.CreatedOn);
                    Assert.Equal("Bratislav", bratislavMetulsky.FirstName);
                    Assert.Equal("Metulsky", bratislavMetulsky.LastName);
                    Assert.Equal("whatever", bratislavMetulsky.Bio);

                    bratislavMetulsky = await repo.SingleOrDefaultAsync(555);
                    Assert.NotNull(bratislavMetulsky);
                    Assert.Equal(scope.TenantId.Value, bratislavMetulsky.TenantId);
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
            using (var scope = _fixture.BeginScope())
            {
                scope.DbConnection.Execute(
                    "INSERT INTO Bloggers (Id, TenantId, CreatedOn, CreatedBy, FirstName, LastName, Bio) " +
                    $"VALUES (456, {scope.TenantId.Value}, '2012-05-12 23:12:09', 'the test', 'Bratislav', 'Metulsky', 'whatever')");
            }

            using (var scope = _fixture.BeginScope())
            {
                var repo = scope.GetRepository<Blogger>();
                Blogger bratislavMetulsky = repo.Single(456);
                bratislavMetulsky.FirstName = "Johnny";
                bratislavMetulsky.LastName = "Flash";
                bratislavMetulsky.Bio = "Der lustige Clown";
            }

            using (var scope = _fixture.BeginScope())
            {
                var count = scope.DbConnection.ExecuteScalar<int>(
                    $"SELECT Count(*) FROM Bloggers WHERE FirstName = 'Johnny' AND LastName = 'Flash' AND TenantId = '{scope.TenantId.Value}'");
                Assert.Equal(1, count);
            }

            using (var scope = _fixture.BeginScope())
            {
                var repo = scope.GetRepository<Blogger>();
                Blogger johnnyFlash = repo.Single(456);
                Assert.Equal(DateTime.UtcNow, johnnyFlash.ChangedOn, new TolerantDateTimeComparer(TimeSpan.FromMilliseconds(5000)));
                Assert.Equal(scope.IdentityHolder.Current.Name, johnnyFlash.ChangedBy);
                Assert.Equal("Johnny", johnnyFlash.FirstName);
                Assert.Equal("Flash", johnnyFlash.LastName);
            }
        }
    }
}