namespace Backend.Fx.EfCorePersistence.Tests
{
    using System;
    using DummyImpl;
    using DummyImpl.Mapping;
    using Environment.Authentication;
    using Environment.DateAndTime;
    using Environment.MultiTenancy;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using Patterns.Authorization;
    using Patterns.DependencyInjection;
    using Testing;
    using Xunit;

    public class TheRepositoryOfPlainAggregate : IDisposable
    {
        private readonly SqliteConnection connection;
        private readonly DbContextOptions dbContextOptions;
        private readonly ICurrentTHolder<TenantId> tenantIdHolder = new CurrentTenantIdHolder();
        private readonly IClock clock = new FrozenClock();

        public TheRepositoryOfPlainAggregate()
        {
            tenantIdHolder.ReplaceCurrent(new TenantId(12));
            connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            dbContextOptions = new DbContextOptionsBuilder().UseSqlite(connection).Options;

            using (var dbContext = new BlogDbContext(dbContextOptions))
            {
                dbContext.Database.EnsureCreated();
            }
        }

        [Fact]
        public void CanCreate()
        {
            using (var dbContext = new BlogDbContext(dbContextOptions))
            {
                using (var uow = new EfUnitOfWork(clock, new SystemIdentity(), dbContext))
                {
                    uow.Begin();
                    var repo = new EfRepository<Blogger>(uow, dbContext, new BloggerMapping(), tenantIdHolder, new AllowAll<Blogger>());
                    repo.Add(new Blogger("Metulsky", "Bratislav"));
                    uow.Complete();
                }
            }

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Count(*) FROM Bloggers";
            long count = (long)cmd.ExecuteScalar();
            Assert.Equal(1L, count);

            cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Count(*) FROM Bloggers WHERE FirstName = 'Bratislav' AND LastName = 'Metulsky' AND TenantId = '12'";
            count = (long)cmd.ExecuteScalar();
            Assert.Equal(1L, count);
        }

        [Fact]
        public void CanRead()
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Bloggers (TenantId, CreatedOn, CreatedBy, Version, FirstName, LastName, Bio) " +
                              "VALUES (12, '2012-05-12 23:12:09', 'the test', '1', 'Bratislav', 'Metulsky', 'whatever')";
            cmd.ExecuteNonQuery();
            
            using (var dbContext = new BlogDbContext(dbContextOptions))
            {
                using (var uow = new EfUnitOfWork(clock, new SystemIdentity(), dbContext))
                {
                    uow.Begin();
                    var repo = new EfRepository<Blogger>(uow, dbContext, new BloggerMapping(), tenantIdHolder, new AllowAll<Blogger>());
                    Blogger bratislavMetulsky = repo.Single(1);
                    Assert.Equal(12, bratislavMetulsky.TenantId);
                    Assert.Equal("the test", bratislavMetulsky.CreatedBy);
                    Assert.Equal(new DateTime(2012,05,12,23,12,09), bratislavMetulsky.CreatedOn);
                    Assert.Equal("Bratislav", bratislavMetulsky.FirstName);
                    Assert.Equal("Metulsky", bratislavMetulsky.LastName);
                    Assert.Equal("whatever", bratislavMetulsky.Bio);
                    uow.Complete();
                }
            }
        }

        [Fact]
        public void CanDelete()
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Bloggers (TenantId, CreatedOn, CreatedBy, Version, FirstName, LastName, Bio) " +
                              "VALUES (12, '2012-05-12 23:12:09', 'the test', '1', 'Bratislav', 'Metulsky', 'whatever')";
            cmd.ExecuteNonQuery();

            using (var dbContext = new BlogDbContext(dbContextOptions))
            {
                using (var uow = new EfUnitOfWork(clock, new SystemIdentity(), dbContext))
                {
                    uow.Begin();
                    var repo = new EfRepository<Blogger>(uow, dbContext, new BloggerMapping(), tenantIdHolder, new AllowAll<Blogger>());
                    Blogger bratislavMetulsky = repo.Single(1);
                    repo.Delete(bratislavMetulsky);
                    uow.Complete();
                }
            }

            cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Count(*) FROM Bloggers";
            long count = (long)cmd.ExecuteScalar();
            Assert.Equal(0L, count);
        }

        [Fact]
        public void CanUpdate()
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Bloggers (TenantId, CreatedOn, CreatedBy, Version, FirstName, LastName, Bio) " +
                              "VALUES (12, '2012-05-12 23:12:09', 'the test', '1', 'Bratislav', 'Metulsky', 'whatever')";
            cmd.ExecuteNonQuery();

            using (var dbContext = new BlogDbContext(dbContextOptions))
            {
                using (var uow = new EfUnitOfWork(clock, new SystemIdentity(), dbContext))
                {
                    uow.Begin();
                    var repo = new EfRepository<Blogger>(uow, dbContext, new BloggerMapping(), tenantIdHolder, new AllowAll<Blogger>());
                    Blogger bratislavMetulsky = repo.Single(1);
                    bratislavMetulsky.FirstName = "Johnny";
                    bratislavMetulsky.LastName = "Flash";
                    bratislavMetulsky.Bio = "Der lustige Clown";
                    uow.Complete();
                }
            }

            cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Count(*) FROM Bloggers WHERE FirstName = 'Johnny' AND LastName = 'Flash' AND TenantId = '12'";
            var count = (long)cmd.ExecuteScalar();
            Assert.Equal(1L, count);

            using (var dbContext = new BlogDbContext(dbContextOptions))
            {
                using (var uow = new EfUnitOfWork(clock, new SystemIdentity(), dbContext))
                {
                    uow.Begin();
                    var repo = new EfRepository<Blogger>(uow, dbContext, new BloggerMapping(), tenantIdHolder, new AllowAll<Blogger>());
                    Blogger johnnyFlash = repo.Single(1);
                    Assert.Equal(clock.UtcNow, johnnyFlash.ChangedOn, new TolerantDateTimeComparer(TimeSpan.FromMilliseconds(1000)));
                    Assert.Equal(new SystemIdentity().Name, johnnyFlash.ChangedBy);
                    Assert.Equal("Johnny", johnnyFlash.FirstName);
                    Assert.Equal("Flash", johnnyFlash.LastName);
                    uow.Complete();
                }
            }
        }

        public void Dispose()
        {
            connection?.Close();
            connection?.Dispose();
        }
    }
}
