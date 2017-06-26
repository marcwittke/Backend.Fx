namespace Backend.Fx.EfCorePersistence.Tests
{
    using System;
    using System.Linq;
    using BuildingBlocks;
    using DummyImpl;
    using Environment.Authentication;
    using Environment.DateAndTime;
    using Environment.MultiTenancy;
    using FakeItEasy;
    using Microsoft.EntityFrameworkCore;
    using Patterns.Authorization;
    using Patterns.IdGeneration;
    using Xunit;

    public class TheRepositoryOfComposedAggregate : TestWithInMemorySqliteDbContext
    {
        private readonly IEntityIdGenerator idGenerator = A.Fake<IEntityIdGenerator>();
        private int nextId = 1;

        public TheRepositoryOfComposedAggregate()
        {
            CreateDatabase();
            A.CallTo(() => idGenerator.NextId()).ReturnsLazily(() => nextId++);
        }

        [Fact]
        public void CanCreate()
        {
            long count = ExecuteScalar<long>("SELECT count(*) FROM Blogs");
            Assert.Equal(0, count);

            count = ExecuteScalar<long>("SELECT count(*) FROM Post");
            Assert.Equal(0, count);

            using (var sut = new SystemUnderTest(DbContextOptions, Clock, TenantId))
            {
                var repository = sut.Repository;
                var blog = new Blog(123, "my blog");
                blog.AddPost(idGenerator, "my post");
                repository.Add(blog);
            }

            count = ExecuteScalar<long>("SELECT count(*) FROM Blogs");
            Assert.Equal(1, count);

            count = ExecuteScalar<long>("SELECT count(*) FROM Post");
            Assert.Equal(1, count);
        }

        [Fact]
        public void CanRead()
        {
            var id = CreateBlogWithPost();
            Blog blog;
            using (var sut = new SystemUnderTest(DbContextOptions, Clock, TenantId))
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

            using (var sut = new SystemUnderTest(DbContextOptions, Clock, TenantId))
            {
                var blog = sut.Repository.Single(id);
                blog.Modify("modified");
            }

            Assert.Equal(1, ExecuteScalar<long>("SELECT count(*) FROM Blogs"));
            Assert.Equal(id, ExecuteScalar<long>("SELECT Id FROM Blogs LIMIT 1"));
            Assert.Equal("modified", ExecuteScalar<string>("SELECT Name FROM Blogs LIMIT 1"));
            Assert.Equal("modified", ExecuteScalar<string>("SELECT Name FROM Post LIMIT 1"));
        }

        [Fact]
        public void CanDelete()
        {
            var id = CreateBlogWithPost();

            using (var sut = new SystemUnderTest(DbContextOptions, Clock, TenantId))
            {
                var blog = sut.Repository.Single(id);
                sut.Repository.Delete(blog);
            }

            long count = ExecuteScalar<long>("SELECT count(*) FROM Blogs");
            Assert.Equal(0, count);

            count = ExecuteScalar<long>("SELECT count(*) FROM Post");
            Assert.Equal(0, count);
        }

        [Fact]
        public void CanDeleteDependant()
        {
            int id = CreateBlogWithPost(10);
            using (var sut = new SystemUnderTest(DbContextOptions, Clock, TenantId))
            {
                var blog = sut.Repository.Single(id);
                blog.Posts.Remove(blog.Posts.First());
            }

            long count = ExecuteScalar<long>("SELECT count(*) FROM Post");
            Assert.Equal(9, count);
        }

        [Fact]
        public void CanUpdateDependant()
        {
            int id = CreateBlogWithPost(10);
            Post post;
            using (var sut = new SystemUnderTest(DbContextOptions, Clock, TenantId))
            {
                var blog = sut.Repository.Single(id);
                post = blog.Posts.First();
                post.SetName("modified");
            }

            string name = ExecuteScalar<string>($"SELECT name FROM Post where id = {post.Id}");
            Assert.Equal("modified", name);
        }

        [Fact]
        public void CanAddDependant()
        {
            int id = CreateBlogWithPost(10);
            using (var sut = new SystemUnderTest(DbContextOptions, Clock, TenantId))
            {
                var blog = sut.Repository.Single(id);
                blog.Posts.Add(new Post(idGenerator.NextId(), blog, "added"));
            }


            long count = ExecuteScalar<long>("SELECT count(*) FROM Post");
            Assert.Equal(11, count);
        }

        [Fact]
        public void CanReplaceDependentCollection()
        {
            var id = CreateBlogWithPost(10);
            using (var sut = new SystemUnderTest(DbContextOptions, Clock, TenantId))
            {
                var blog = sut.Repository.Single(id);
                blog.Posts.Clear();
                blog.Posts.Add(new Post(idGenerator.NextId(), blog, "new name 1"));
                blog.Posts.Add(new Post(idGenerator.NextId(), blog, "new name 2"));
                blog.Posts.Add(new Post(idGenerator.NextId(), blog, "new name 3"));
                blog.Posts.Add(new Post(idGenerator.NextId(), blog, "new name 4"));
                blog.Posts.Add(new Post(idGenerator.NextId(), blog, "new name 5"));
            }

            long count = ExecuteScalar<long>("SELECT count(*) FROM Post");
            Assert.Equal(5, count);
        }

        private int CreateBlogWithPost(int postCount = 1)
        {
            long blogId = nextId++;
            ExecuteNonQuery($"INSERT INTO Blogs (Id, TenantId, Name, CreatedOn, CreatedBy) VALUES ({blogId}, {TenantId.Value}, 'my blog', CURRENT_TIMESTAMP, 'persistence test')");
            long count = ExecuteScalar<long>("SELECT count(*) FROM Blogs");
            Assert.Equal(1, count);
            
            for (int i = 0; i < postCount; i++)
            {
                ExecuteNonQuery($"INSERT INTO Post (Id, BlogId, Name, CreatedOn, CreatedBy) VALUES ({nextId++}, {blogId}, 'my post {i:00}', CURRENT_TIMESTAMP, 'persistence test')");
            }

            return (int)blogId;
        }

        private class SystemUnderTest : IDisposable
        {
            public TestDbContext DbContext { get; }
            public EfUnitOfWork UnitOfWork { get; }
            public IRepository<Blog> Repository { get; }

            public SystemUnderTest(DbContextOptions dbContextOptions, IClock clock, TenantId tenantId)
            {
                DbContext = new TestDbContext(dbContextOptions);
                UnitOfWork = new EfUnitOfWork(clock, new SystemIdentity(), DbContext);
                UnitOfWork.Begin();
                Repository = new EfRepository<Blog>(UnitOfWork, DbContext, new BlogMapping(), tenantId, new AllowAll<Blog>());
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
