using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
using Backend.Fx.EfCorePersistence.Tests.Fixtures;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Extensions;
using Backend.Fx.Patterns.Authorization;
using Backend.Fx.Patterns.IdGeneration;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.EfCorePersistence.Tests
{
    public class TheRepositoryOfComposedAggregate
    {
        private static int _nextTenantId = 57839;
        private readonly int _tenantId = _nextTenantId++;
        private readonly IEqualityComparer<DateTime?> _tolerantDateTimeComparer = new TolerantDateTimeComparer(TimeSpan.FromMilliseconds(5000));
        private readonly IEntityIdGenerator _idGenerator = A.Fake<IEntityIdGenerator>();
        private readonly DatabaseFixture _fixture;
        private readonly IClock _clock = new FrozenClock();
        private int _nextId = 1;

        public TheRepositoryOfComposedAggregate()
        {
            A.CallTo(() => _idGenerator.NextId()).ReturnsLazily(() => _nextId++);
            //_fixture = new SqlServerDatabaseFixture();
             _fixture = new SqliteDatabaseFixture();
            _fixture.CreateDatabase();
        }

        [Fact]
        public void CanCreate()
        {
            using (var dbs = _fixture.UseDbSession())
            {
                using (dbs.Connection.OpenDisposable())
                {
                    int count = dbs.Connection.ExecuteScalar<int>("SELECT count(*) FROM Blogs");
                    Assert.Equal(0, count);

                    count = dbs.Connection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                    Assert.Equal(0, count);
                }

                using (var uow = dbs.UseUnitOfWork(_clock))
                {
                    var sut = new EfRepository<Blog>(dbs.DbContext, new BlogMapping(), CurrentTenantIdHolder.Create(_tenantId), new AllowAll<Blog>());
                    var blog = new Blog(_idGenerator.NextId(), "my blog");
                    blog.AddPost(_idGenerator, "my post");
                    sut.Add(blog);
                    uow.Complete();
                }

                using (dbs.Connection.OpenDisposable())
                {
                    int count = dbs.Connection.ExecuteScalar<int>("SELECT count(*) FROM Blogs");
                    Assert.Equal(1, count);

                    count = dbs.Connection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                    Assert.Equal(1, count);
                }
            }
        }

        [Fact]
        public void CanRead()
        {
            using (var dbs = _fixture.UseDbSession())
            {
                var id = CreateBlogWithPost(dbs);
                Blog blog;

                using (var uow = dbs.UseUnitOfWork(_clock))
                {
                    var sut = new EfRepository<Blog>(dbs.DbContext, new BlogMapping(),
                        CurrentTenantIdHolder.Create(_tenantId), new AllowAll<Blog>());
                    blog = sut.Single(id);
                    uow.Complete();
                }

                using (dbs.Connection.OpenDisposable())
                {
                    Assert.NotNull(blog);
                    Assert.Equal(id, blog.Id);
                    Assert.Equal("my blog", blog.Name);
                    Assert.NotEmpty(blog.Posts);
                }
            }
        }

        [Fact]
        public void CanUpdate()
        {
            using (var dbs = _fixture.UseDbSession())
            {
                var id = CreateBlogWithPost(dbs);

                using (var uow = dbs.UseUnitOfWork(_clock))
                {
                    var sut = new EfRepository<Blog>(dbs.DbContext, new BlogMapping(),
                        CurrentTenantIdHolder.Create(_tenantId), new AllowAll<Blog>());
                    var blog = sut.Single(id);
                    blog.Modify("modified");
                    uow.Complete();
                }

                using (dbs.Connection.OpenDisposable())
                {
                    Assert.Equal(1, dbs.Connection.ExecuteScalar<int>("SELECT count(*) FROM Blogs"));
                    Assert.Equal(id, dbs.Connection.ExecuteScalar<int>("SELECT Id FROM Blogs LIMIT 1"));
                    Assert.Equal("modified", dbs.Connection.ExecuteScalar<string>("SELECT Name FROM Blogs LIMIT 1"));
                    Assert.Equal("modified", dbs.Connection.ExecuteScalar<string>("SELECT Name FROM Posts LIMIT 1"));
                }
            }
        }

        [Fact]
        public void CanDelete()
        {
            using (var dbs = _fixture.UseDbSession())
            {
                var id = CreateBlogWithPost(dbs);

                using (var uow = dbs.UseUnitOfWork(_clock))
                {
                    var sut = new EfRepository<Blog>(dbs.DbContext, new BlogMapping(), CurrentTenantIdHolder.Create(_tenantId), new AllowAll<Blog>());
                    var blog = sut.Single(id);
                    sut.Delete(blog);
                    uow.Complete();
                }

                using (dbs.Connection.OpenDisposable())
                {
                    int count = dbs.Connection.ExecuteScalar<int>("SELECT count(*) FROM Blogs");
                    Assert.Equal(0, count);

                    count = dbs.Connection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                    Assert.Equal(0, count);
                }
            }
        }

        [Fact]
        public void CanDeleteDependant()
        {
            using (var dbs = _fixture.UseDbSession())
            {
                int id = CreateBlogWithPost(dbs, 10);

                using (dbs.Connection.OpenDisposable())
                {
                    int count = dbs.Connection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                    Assert.Equal(10, count);
                }

                using (var uow = dbs.UseUnitOfWork(_clock))
                {
                    var sut = new EfRepository<Blog>(dbs.DbContext, new BlogMapping(),
                        CurrentTenantIdHolder.Create(_tenantId), new AllowAll<Blog>());
                    var blog = sut.Single(id);
                    var firstPost = blog.Posts.First();
                    firstPost.SetName("sadfasfsadf");
                    blog.Posts.Remove(firstPost);
                    uow.Complete();
                }

                using (dbs.Connection.OpenDisposable())
                {
                    var count = dbs.Connection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                    Assert.Equal(9, count);
                }
            }
        }

        [Fact]
        public void CanUpdateDependant()
        {
            using (var dbs = _fixture.UseDbSession())
            {
                int id = CreateBlogWithPost(dbs, 10);
                Post post;

                using (var uow = dbs.UseUnitOfWork(_clock))
                {
                    var sut = new EfRepository<Blog>(dbs.DbContext, new BlogMapping(), CurrentTenantIdHolder.Create(_tenantId),
                        new AllowAll<Blog>());
                    var blog = sut.Single(id);
                    post = blog.Posts.First();
                    post.SetName("modified");
                    uow.Complete();
                }

                using (dbs.Connection.OpenDisposable())
                {
                    string name = dbs.Connection.ExecuteScalar<string>($"SELECT name FROM Posts where id = {post.Id}");
                    Assert.Equal("modified", name);
                }
            }
        }

        [Fact]
        public void CanAddDependant()
        {
            using (var dbs = _fixture.UseDbSession())
            {
                int id = CreateBlogWithPost(dbs, 10);

                using (var uow = dbs.UseUnitOfWork(_clock))
                {
                    var sut = new EfRepository<Blog>(dbs.DbContext, new BlogMapping(), CurrentTenantIdHolder.Create(_tenantId), new AllowAll<Blog>());
                    var blog = sut.Single(id);
                    blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "added"));
                    uow.Complete();
                }

                using (dbs.Connection.OpenDisposable())
                {
                    int count = dbs.Connection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                    Assert.Equal(11, count);
                }
            }
        }


        [Fact]
        public void CanReplaceDependentCollection()
        {
            using (var dbs = _fixture.UseDbSession())
            {
                var id = CreateBlogWithPost(dbs, 10);

                using (var uow = dbs.UseUnitOfWork(_clock))
                {
                    var sut = new EfRepository<Blog>(dbs.DbContext, new BlogMapping(), CurrentTenantIdHolder.Create(_tenantId), new AllowAll<Blog>());
                    var blog = sut.Single(id);
                    blog.Posts.Clear();
                    blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "new name 1"));
                    blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "new name 2"));
                    blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "new name 3"));
                    blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "new name 4"));
                    blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "new name 5"));
                    uow.Complete();
                }

                using (dbs.Connection.OpenDisposable())
                {
                    int count = dbs.Connection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                    Assert.Equal(5, count);
                }
            }
        }

        [Fact]
        public void UpdatesAggregateTrackingPropertiesOnDeleteOfDependant()
        {
            using (var dbs = _fixture.UseDbSession())
            {
                int id = CreateBlogWithPost(dbs, 10);

                var expectedModifiedOn = _clock.UtcNow.AddHours(1);
                _clock.OverrideUtcNow(expectedModifiedOn);

                using (var uow = dbs.UseUnitOfWork(_clock))
                {
                    var sut = new EfRepository<Blog>(dbs.DbContext, new BlogMapping(), CurrentTenantIdHolder.Create(_tenantId), new AllowAll<Blog>());
                    var b = sut.Single(id);
                    b.Posts.Remove(b.Posts.First());
                    uow.Complete();
                }

                using (dbs.Connection.OpenDisposable())
                {
                    var blog = dbs.DbContext.Blogs.Find(id);
                    Assert.NotNull(blog.ChangedOn);
                    Assert.Equal(expectedModifiedOn, blog.ChangedOn.Value, _tolerantDateTimeComparer);
                }
            }
        }

        private int CreateBlogWithPost(DbSession dbs, int postCount = 1)
        {
            using (dbs.Connection.OpenDisposable())
            {
                int blogId = _nextId++;
                dbs.Connection.ExecuteNonQuery(
                    $"INSERT INTO Blogs (Id, TenantId, Name, CreatedOn, CreatedBy) VALUES ({blogId}, {CurrentTenantIdHolder.Create(_tenantId).Current.Value}, 'my blog', CURRENT_TIMESTAMP, 'persistence test')");
                int count = dbs.Connection.ExecuteScalar<int>("SELECT count(*) FROM Blogs");
                Assert.Equal(1, count);

                for (int i = 0; i < postCount; i++)
                {
                    dbs.Connection.ExecuteNonQuery(
                        $"INSERT INTO Posts (Id, BlogId, Name, CreatedOn, CreatedBy) VALUES ({_nextId++}, {blogId}, 'my post {i:00}', CURRENT_TIMESTAMP, 'persistence test')");
                }

                return (int) blogId;
            }
        }
    }
}
