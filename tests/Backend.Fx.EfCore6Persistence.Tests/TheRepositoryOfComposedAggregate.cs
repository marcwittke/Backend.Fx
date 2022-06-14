using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Backend.Fx.EfCore6Persistence.Tests.DummyImpl.Domain;
using Backend.Fx.EfCore6Persistence.Tests.DummyImpl.Persistence;
using Backend.Fx.EfCore6Persistence.Tests.Fixtures;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Extensions;
using Backend.Fx.Patterns.Authorization;
using Backend.Fx.Patterns.IdGeneration;
using Backend.Fx.Tests;
using FakeItEasy;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.EfCore6Persistence.Tests
{
    public class TheRepositoryOfComposedAggregate : TestWithLogging
    {
        public TheRepositoryOfComposedAggregate(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            A.CallTo(() => _idGenerator.NextId()).ReturnsLazily(() => _nextId++);
            //_fixture = new SqlServerDatabaseFixture();
            _fixture = new SqliteDatabaseFixture();
            _fixture.CreateDatabase();
        }

        private static int _nextTenantId = 57839;
        private static int _nextId = 1;
        private readonly int _tenantId = _nextTenantId++;

        private readonly IEqualityComparer<DateTime?> _tolerantDateTimeComparer =
            new TolerantDateTimeComparer(TimeSpan.FromMilliseconds(5000));

        private readonly IEntityIdGenerator _idGenerator = A.Fake<IEntityIdGenerator>();
        private readonly DatabaseFixture _fixture;

        private int CreateBlogWithPost(IDbConnection dbConnection, int postCount = 1)
        {
            {
                var blogId = _nextId++;
                dbConnection.ExecuteNonQuery(
                    $"INSERT INTO Blogs (Id, TenantId, Name, CreatedOn, CreatedBy) VALUES ({blogId}, {CurrentTenantIdHolder.Create(_tenantId).Current.Value}, 'my blog', CURRENT_TIMESTAMP, 'persistence test')");
                var count = dbConnection.ExecuteScalar<int>("SELECT count(*) FROM Blogs");
                Assert.Equal(1, count);

                for (var i = 0; i < postCount; i++)
                    dbConnection.ExecuteNonQuery(
                        $"INSERT INTO Posts (Id, BlogId, Name, TargetAudience_IsPublic, TargetAudience_Culture, CreatedOn, CreatedBy) VALUES ({_nextId++}, {blogId}, 'my post {i:00}', '1', 'de-DE', CURRENT_TIMESTAMP, 'persistence test')");

                return blogId;
            }
        }

        //FAILING!!!!
        // this shows, that ValueObjects treated as OwnedTypes are not supported very well
        //[Fact]
        //public void CanUpdateDependantValueObject()
        //{
        //    using (DbSession dbs = _fixture.UseDbSession())
        //    {
        //        int id = CreateBlogWithPost(dbSession.DbConnection, 10);
        //        Post post;

        //        using (var uow = dbs.UseUnitOfWork(_clock))
        //        {
        //            var sut = new EfRepository<Blog>(uow.DbContext, new BlogMapping(), CurrentTenantIdHolder.Create(_tenantId),
        //                new AllowAll<Blog>());
        //            var blog = sut.Single(id);
        //            post = blog.Posts.First();
        //            post.TargetAudience = new TargetAudience{Culture = "es-AR", IsPublic = false};
        //            uow.Complete();
        //        }

        //        
        //        {
        //            string culture = dbSession.DbConnection.ExecuteScalar<string>($"SELECT TargetAudience_Culture ame FROM Posts where id = {post.Id}");
        //            Assert.Equal("es-AR", culture);

        //            string strChangedOn = dbSession.DbConnection.ExecuteScalar<string>($"SELECT ChangedOn FROM Posts where id = {post.Id}");
        //            DateTime changedOn = DateTime.Parse(strChangedOn);
        //            Assert.Equal(_clock.UtcNow, changedOn, new TolerantDateTimeComparer(TimeSpan.FromMilliseconds(500)));
        //        }
        //    }
        //}

        [Fact]
        public void CanAddDependent()
        {
            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                var id = CreateBlogWithPost(dbSession.DbConnection, 10);
                var sut = new EfRepository<Blog>(dbSession.DbContext,
                    new BlogMapping(),
                    CurrentTenantIdHolder.Create(_tenantId),
                    new AllowAll<Blog>());
                Blog blog = sut.Single(id);
                blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "added"));
            }

            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                var count = dbSession.DbConnection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                Assert.Equal(11, count);
            }
        }

        [Fact]
        public void CanCreate()
        {
            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                var count = dbSession.DbConnection.ExecuteScalar<int>("SELECT count(*) FROM Blogs");
                Assert.Equal(0, count);

                count = dbSession.DbConnection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                Assert.Equal(0, count);
            }

            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                var sut = new EfRepository<Blog>(dbSession.DbContext,
                    new BlogMapping(),
                    CurrentTenantIdHolder.Create(_tenantId),
                    new AllowAll<Blog>());
                var blog = new Blog(_idGenerator.NextId(), "my blog");
                blog.AddPost(_idGenerator, "my post");
                sut.Add(blog);
            }

            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                var count = dbSession.DbConnection.ExecuteScalar<int>("SELECT count(*) FROM Blogs");
                Assert.Equal(1, count);

                count = dbSession.DbConnection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                Assert.Equal(1, count);
            }
        }

        [Fact]
        public void CanDelete()
        {
            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                var id = CreateBlogWithPost(dbSession.DbConnection);

                var sut = new EfRepository<Blog>(dbSession.DbContext,
                    new BlogMapping(),
                    CurrentTenantIdHolder.Create(_tenantId),
                    new AllowAll<Blog>());
                Blog blog = sut.Single(id);
                sut.Delete(blog);
            }

            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                var count = dbSession.DbConnection.ExecuteScalar<int>("SELECT count(*) FROM Blogs");
                Assert.Equal(0, count);

                count = dbSession.DbConnection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                Assert.Equal(0, count);
            }
        }

        [Fact]
        public void CanDeleteDependent()
        {
            int id;
            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                id = CreateBlogWithPost(dbSession.DbConnection, 10);
                var count = dbSession.DbConnection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                Assert.Equal(10, count);
            }

            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                var sut = new EfRepository<Blog>(dbSession.DbContext,
                    new BlogMapping(),
                    CurrentTenantIdHolder.Create(_tenantId),
                    new AllowAll<Blog>());
                Blog blog = sut.Single(id);
                Post firstPost = blog.Posts.First();
                firstPost.SetName("sadfasfsadf");
                blog.Posts.Remove(firstPost);
            }

            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                var count = dbSession.DbConnection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                Assert.Equal(9, count);
            }
        }


        [Fact]
        public void CanRead()
        {
            int id;
            Blog blog;

            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                id = CreateBlogWithPost(dbSession.DbConnection);
            }

            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                var sut = new EfRepository<Blog>(dbSession.DbContext,
                    new BlogMapping(),
                    CurrentTenantIdHolder.Create(_tenantId),
                    new AllowAll<Blog>());
                blog = sut.Single(id);
            }

            Assert.NotNull(blog);
            Assert.Equal(id, blog.Id);
            Assert.Equal("my blog", blog.Name);
            Assert.NotEmpty(blog.Posts);
        }


        [Fact]
        public void CanReplaceDependentCollection()
        {
            int id;
            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                id = CreateBlogWithPost(dbSession.DbConnection, 10);
            }

            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                var sut = new EfRepository<Blog>(dbSession.DbContext,
                    new BlogMapping(),
                    CurrentTenantIdHolder.Create(_tenantId),
                    new AllowAll<Blog>());
                Blog blog = sut.Single(id);
                blog.Posts.Clear();
                blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "new name 1"));
                blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "new name 2"));
                blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "new name 3"));
                blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "new name 4"));
                blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "new name 5"));
            }

            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                var count = dbSession.DbConnection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                Assert.Equal(5, count);
            }
        }

        [Fact]
        public void CanUpdate()
        {
            int id;
            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                id = CreateBlogWithPost(dbSession.DbConnection);
            }

            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                var sut = new EfRepository<Blog>(dbSession.DbContext,
                    new BlogMapping(),
                    CurrentTenantIdHolder.Create(_tenantId),
                    new AllowAll<Blog>());
                Blog blog = sut.Single(id);
                blog.Modify("modified");
            }

            using (TestDbSession dbSession = _fixture.CreateTestDbSession())
            {
                Assert.Equal(1, dbSession.DbConnection.ExecuteScalar<int>("SELECT count(*) FROM Blogs"));
                Assert.Equal(id, dbSession.DbConnection.ExecuteScalar<int>("SELECT Id FROM Blogs LIMIT 1"));
                Assert.Equal("modified", dbSession.DbConnection.ExecuteScalar<string>("SELECT Name FROM Blogs LIMIT 1"));
                Assert.Equal("modified", dbSession.DbConnection.ExecuteScalar<string>("SELECT Name FROM Posts LIMIT 1"));
            }
        }

        [Fact]
        public void CanUpdateDependant()
        {
            var clock = new AdjustableClock(new WallClock());
            clock.OverrideUtcNow(new DateTime(2020, 01, 20, 20, 30, 40));

            int id;
            using (TestDbSession dbSession = _fixture.CreateTestDbSession(clock: clock))
            {
                id = CreateBlogWithPost(dbSession.DbConnection, 10);
            }

            Post post;

            using (TestDbSession dbSession = _fixture.CreateTestDbSession(clock: clock))
            {
                var sut = new EfRepository<Blog>(dbSession.DbContext,
                    new BlogMapping(),
                    CurrentTenantIdHolder.Create(_tenantId),
                    new AllowAll<Blog>());
                Blog blog = sut.Single(id);
                post = blog.Posts.First();
                post.SetName("modified");
            }

            using (TestDbSession dbSession = _fixture.CreateTestDbSession(clock: clock))
            {
                var name = dbSession.DbConnection.ExecuteScalar<string>($"SELECT name FROM Posts where id = {post.Id}");
                Assert.Equal("modified", name);

                var strChangedOn = dbSession.DbConnection.ExecuteScalar<string>($"SELECT changedon FROM Posts where id = {post.Id}");
                DateTime changedOn = DateTime.Parse(strChangedOn);
                Assert.Equal(clock.UtcNow, changedOn, new TolerantDateTimeComparer(TimeSpan.FromMilliseconds(500)));
            }
        }

        [Fact]
        public void UpdatesAggregateTrackingPropertiesOnDeleteOfDependant()
        {
            var clock = new AdjustableClock(new WallClock());
            clock.OverrideUtcNow(new DateTime(2020, 01, 20, 20, 30, 40));

            int id;
            using (TestDbSession dbSession = _fixture.CreateTestDbSession(clock: clock))
            {
                id = CreateBlogWithPost(dbSession.DbConnection, 10);
            }

            DateTime expectedModifiedOn = clock.Advance(TimeSpan.FromHours(1));

            using (TestDbSession dbSession = _fixture.CreateTestDbSession(clock: clock))
            {
                var sut = new EfRepository<Blog>(dbSession.DbContext,
                    new BlogMapping(),
                    CurrentTenantIdHolder.Create(_tenantId),
                    new AllowAll<Blog>());
                Blog b = sut.Single(id);
                b.Posts.Remove(b.Posts.First());
            }

            using (TestDbSession dbSession = _fixture.CreateTestDbSession(clock: clock))
            {
                Blog blog = dbSession.DbContext.Set<Blog>().Find(id);
                Assert.NotNull(blog.ChangedOn);
                Assert.Equal(expectedModifiedOn, blog.ChangedOn.Value, _tolerantDateTimeComparer);
            }
        }
    }
}