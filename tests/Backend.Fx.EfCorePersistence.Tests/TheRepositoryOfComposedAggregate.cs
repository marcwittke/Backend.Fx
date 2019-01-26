using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Extensions;
using Backend.Fx.Patterns.Authorization;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.Patterns.IdGeneration;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Backend.Fx.EfCorePersistence.Tests
{
    public class TheRepositoryOfComposedAggregate : TestWithInMemorySqliteDbContext
    {
        private readonly IEqualityComparer<DateTime?> _tolerantDateTimeComparer = new TolerantDateTimeComparer(TimeSpan.FromMilliseconds(500));
        private readonly IEntityIdGenerator _idGenerator = A.Fake<IEntityIdGenerator>();
        private int _nextId = 1;

        public TheRepositoryOfComposedAggregate()
        {
            CreateDatabase();
            A.CallTo(() => _idGenerator.NextId()).ReturnsLazily(() => _nextId++);
        }

        [Fact]
        public void CanCreate()
        {
            long count = ExecuteScalar<long>("SELECT count(*) FROM Blogs");
            Assert.Equal(0, count);

            count = ExecuteScalar<long>("SELECT count(*) FROM Posts");
            Assert.Equal(0, count);

            using (var sut = new SystemUnderTest(DbContextOptions(), Clock, TenantIdHolder, Connection))
            {
                var repository = sut.Repository;
                var blog = new Blog(123, "my blog");
                blog.AddPost(_idGenerator, "my post");
                repository.Add(blog);
            }

            count = ExecuteScalar<long>("SELECT count(*) FROM Blogs");
            Assert.Equal(1, count);

            count = ExecuteScalar<long>("SELECT count(*) FROM Posts");
            Assert.Equal(1, count);
        }

        [Fact]
        public void CanRead()
        {
            var id = CreateBlogWithPost();
            Blog blog;
            using (var sut = new SystemUnderTest(DbContextOptions(), Clock, TenantIdHolder, Connection))
            {
                blog = sut.Repository.Single(id);
            }

            Assert.NotNull(blog);
            Assert.Equal(id, blog.Id);
            Assert.Equal("my blog", blog.Name);
            Assert.NotEmpty(blog.Posts);
        }

        [Fact]
        public void CanUpdate()
        {
            var id = CreateBlogWithPost();

            using (var sut = new SystemUnderTest(DbContextOptions(), Clock, TenantIdHolder, Connection))
            {
                var blog = sut.Repository.Single(id);
                blog.Modify("modified");
            }

            Assert.Equal(1, ExecuteScalar<long>("SELECT count(*) FROM Blogs"));
            Assert.Equal(id, ExecuteScalar<long>("SELECT Id FROM Blogs LIMIT 1"));
            Assert.Equal("modified", ExecuteScalar<string>("SELECT Name FROM Blogs LIMIT 1"));
            Assert.Equal("modified", ExecuteScalar<string>("SELECT Name FROM Posts LIMIT 1"));
        }

        [Fact]
        public void CanDelete()
        {
            var id = CreateBlogWithPost();

            using (var sut = new SystemUnderTest(DbContextOptions(), Clock, TenantIdHolder, Connection))
            {
                var blog = sut.Repository.Single(id);
                sut.Repository.Delete(blog);
            }

            long count = ExecuteScalar<long>("SELECT count(*) FROM Blogs");
            Assert.Equal(0, count);

            count = ExecuteScalar<long>("SELECT count(*) FROM Posts");
            Assert.Equal(0, count);
        }

        [Fact]
        public void CanDeleteDependant()
        {
            int id = CreateBlogWithPost(10);
            long count = ExecuteScalar<long>("SELECT count(*) FROM Posts");
            Assert.Equal(10, count);

            using (var sut = new SystemUnderTest(DbContextOptions(), Clock, TenantIdHolder, Connection))
            {
                var blog = sut.Repository.Single(id);
                var firstPost = blog.Posts.First();
                firstPost.SetName("sadfasfsadf");
                blog.Posts.Remove(firstPost);
            }

            count = ExecuteScalar<long>("SELECT count(*) FROM Posts");
            Assert.Equal(9, count);
        }

        [Fact]
        public void CanUpdateDependant()
        {
            int id = CreateBlogWithPost(10);
            Post post;
            using (var sut = new SystemUnderTest(DbContextOptions(), Clock, TenantIdHolder, Connection))
            {
                var blog = sut.Repository.Single(id);
                post = blog.Posts.First();
                post.SetName("modified");
            }

            string name = ExecuteScalar<string>($"SELECT name FROM Posts where id = {post.Id}");
            Assert.Equal("modified", name);
        }

        [Fact]
        public void CanAddDependant()
        {
            int id = CreateBlogWithPost(10);
            using (var sut = new SystemUnderTest(DbContextOptions(), Clock, TenantIdHolder, Connection))
            {
                var blog = sut.Repository.Single(id);
                blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "added"));
            }


            long count = ExecuteScalar<long>("SELECT count(*) FROM Posts");
            Assert.Equal(11, count);
        }

        [Fact]
        public void CanReplaceDependentCollection()
        {
            var id = CreateBlogWithPost(10);
            using (var sut = new SystemUnderTest(DbContextOptions(), Clock, TenantIdHolder, Connection))
            {
                var blog = sut.Repository.Single(id);
                blog.Posts.Clear();
                blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "new name 1"));
                blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "new name 2"));
                blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "new name 3"));
                blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "new name 4"));
                blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "new name 5"));
            }

            long count = ExecuteScalar<long>("SELECT count(*) FROM Posts");
            Assert.Equal(5, count);
        }

        [Fact]
        public void UpdatesAggregateTrackingPropertiesOnDeleteOfDependant()
        {
            int id = CreateBlogWithPost(10);

            var expectedModifiedOn = Clock.UtcNow.AddHours(1);
            Clock.OverrideUtcNow(expectedModifiedOn);

            using (var sut = new SystemUnderTest(DbContextOptions(), Clock, TenantIdHolder, Connection))
            {
                var blog = sut.Repository.Single(id);
                blog.Posts.Remove(blog.Posts.First());
            }

            Clock.OverrideUtcNow(Clock.UtcNow.AddHours(1));

            using (var sut = new SystemUnderTest(DbContextOptions(), Clock, TenantIdHolder, Connection))
            {
                var blog = sut.DbContext.Blogs.Find(id);
                Assert.NotNull(blog.ChangedOn);
                Assert.Equal(expectedModifiedOn, blog.ChangedOn.Value, _tolerantDateTimeComparer);
            }
        }

        private int CreateBlogWithPost(int postCount = 1)
        {
            long blogId = _nextId++;
            ExecuteNonQuery($"INSERT INTO Blogs (Id, TenantId, Name, CreatedOn, CreatedBy) VALUES ({blogId}, {TenantIdHolder.Current.Value}, 'my blog', CURRENT_TIMESTAMP, 'persistence test')");
            long count = ExecuteScalar<long>("SELECT count(*) FROM Blogs");
            Assert.Equal(1, count);
            
            for (int i = 0; i < postCount; i++)
            {
                ExecuteNonQuery($"INSERT INTO Posts (Id, BlogId, Name, CreatedOn, CreatedBy) VALUES ({_nextId++}, {blogId}, 'my post {i:00}', CURRENT_TIMESTAMP, 'persistence test')");
            }

            return (int)blogId;
        }

        private class SystemUnderTest : IDisposable
        {
            public TestDbContext DbContext { get; }
            public EfUnitOfWork UnitOfWork { get; }
            public IRepository<Blog> Repository { get; }

            public SystemUnderTest(DbContextOptions<TestDbContext> dbContextOptions, IClock clock,
                ICurrentTHolder<TenantId> tenantIdHolder, IDbConnection connection)
            {
                DbContext = new TestDbContext(dbContextOptions);
                UnitOfWork = new EfUnitOfWork(clock, CurrentIdentityHolder.CreateSystem(), A.Fake<IDomainEventAggregator>(), 
                                              A.Fake<IEventBusScope>(), DbContext, connection);
                UnitOfWork.Begin();
                Repository = new EfRepository<Blog>(DbContext, new BlogMapping(), tenantIdHolder, new AllowAll<Blog>());
            }

            public void Dispose()
            {
                DbContext.TraceChangeTrackerState();
                UnitOfWork.Complete();
                UnitOfWork.Dispose();
                DbContext.Dispose();
            }
        }
    }
}
