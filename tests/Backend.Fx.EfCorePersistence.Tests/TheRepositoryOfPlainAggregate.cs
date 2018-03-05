namespace Backend.Fx.EfCorePersistence.Tests
{
    using System;
    using DummyImpl;
    using Environment.Authentication;
    using Patterns.Authorization;
    using Testing;
    using Xunit;

    public class TheRepositoryOfPlainAggregate : TestWithInMemorySqliteDbContext
    {
        public TheRepositoryOfPlainAggregate()
        {
            CreateDatabase();
        }

        [Fact]
        public void CanCreate()
        {
            using (var dbContext = new TestDbContext(DbContextOptions))
            {
                using (var uow = new EfUnitOfWork(Clock, CurrentIdentityHolder.CreateSystem(), dbContext))
                {
                    uow.Begin();
                    var repo = new EfRepository<Blogger>(dbContext, new BloggerMapping(), TenantIdHolder, new AllowAll<Blogger>());
                    repo.Add(new Blogger(345, "Metulsky", "Bratislav"));
                    uow.Complete();
                }
            }

            var cmd = Connection.CreateCommand();
            cmd.CommandText = "SELECT Count(*) FROM Bloggers";
            long count = (long)cmd.ExecuteScalar();
            Assert.Equal(1L, count);

            cmd = Connection.CreateCommand();
            cmd.CommandText = "SELECT Count(*) FROM Bloggers WHERE FirstName = 'Bratislav' AND LastName = 'Metulsky' AND TenantId = '12'";
            count = (long)cmd.ExecuteScalar();
            Assert.Equal(1L, count);
        }

        [Fact]
        public void CanRead()
        {
            var cmd = Connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Bloggers (Id, TenantId, CreatedOn, CreatedBy, FirstName, LastName, Bio) " +
                              "VALUES (444, 12, '2012-05-12 23:12:09', 'the test', 'Bratislav', 'Metulsky', 'whatever')";
            cmd.ExecuteNonQuery();
            
            using (var dbContext = new TestDbContext(DbContextOptions))
            {
                using (var uow = new EfUnitOfWork(Clock, CurrentIdentityHolder.CreateSystem(), dbContext))
                {
                    uow.Begin();
                    var repo = new EfRepository<Blogger>(dbContext, new BloggerMapping(), TenantIdHolder, new AllowAll<Blogger>());
                    Blogger bratislavMetulsky = repo.Single(444);
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
            var cmd = Connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Bloggers (Id, TenantId, CreatedOn, CreatedBy, FirstName, LastName, Bio) " +
                              "VALUES (555, 12, '2012-05-12 23:12:09', 'the test', 'Bratislav', 'Metulsky', 'whatever')";
            cmd.ExecuteNonQuery();

            using (var dbContext = new TestDbContext(DbContextOptions))
            {
                using (var uow = new EfUnitOfWork(Clock, CurrentIdentityHolder.CreateSystem(), dbContext))
                {
                    uow.Begin();
                    var repo = new EfRepository<Blogger>(dbContext, new BloggerMapping(), TenantIdHolder, new AllowAll<Blogger>());
                    Blogger bratislavMetulsky = repo.Single(555);
                    repo.Delete(bratislavMetulsky);
                    uow.Complete();
                }
            }

            cmd = Connection.CreateCommand();
            cmd.CommandText = "SELECT Count(*) FROM Bloggers";
            long count = (long)cmd.ExecuteScalar();
            Assert.Equal(0L, count);
        }

        [Fact]
        public void CanUpdate()
        {
            var cmd = Connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Bloggers (Id, TenantId, CreatedOn, CreatedBy, FirstName, LastName, Bio) " +
                              "VALUES (456, 12, '2012-05-12 23:12:09', 'the test', 'Bratislav', 'Metulsky', 'whatever')";
            cmd.ExecuteNonQuery();

            using (var dbContext = new TestDbContext(DbContextOptions))
            {
                using (var uow = new EfUnitOfWork(Clock, CurrentIdentityHolder.CreateSystem(), dbContext))
                {
                    uow.Begin();
                    var repo = new EfRepository<Blogger>(dbContext, new BloggerMapping(), TenantIdHolder, new AllowAll<Blogger>());
                    Blogger bratislavMetulsky = repo.Single(456);
                    bratislavMetulsky.FirstName = "Johnny";
                    bratislavMetulsky.LastName = "Flash";
                    bratislavMetulsky.Bio = "Der lustige Clown";
                    uow.Complete();
                }
            }

            cmd = Connection.CreateCommand();
            cmd.CommandText = "SELECT Count(*) FROM Bloggers WHERE FirstName = 'Johnny' AND LastName = 'Flash' AND TenantId = '12'";
            var count = (long)cmd.ExecuteScalar();
            Assert.Equal(1L, count);

            using (var dbContext = new TestDbContext(DbContextOptions))
            {
                using (var uow = new EfUnitOfWork(Clock, CurrentIdentityHolder.CreateSystem(), dbContext))
                {
                    uow.Begin();
                    var repo = new EfRepository<Blogger>(dbContext, new BloggerMapping(), TenantIdHolder, new AllowAll<Blogger>());
                    Blogger johnnyFlash = repo.Single(456);
                    Assert.Equal(Clock.UtcNow, johnnyFlash.ChangedOn, new TolerantDateTimeComparer(TimeSpan.FromMilliseconds(1000)));
                    Assert.Equal(new SystemIdentity().Name, johnnyFlash.ChangedBy);
                    Assert.Equal("Johnny", johnnyFlash.FirstName);
                    Assert.Equal("Flash", johnnyFlash.LastName);
                    uow.Complete();
                }
            }
        }

        
    }
}
