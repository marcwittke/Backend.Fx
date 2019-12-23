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
using Backend.Fx.Patterns.UnitOfWork;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.EfCorePersistence.Tests
{
    public class TheRepositoryOfComposedAggregate
    {
        private static int _nextTenantId = 57839;
        private static int _nextId = 1;
        private readonly int _tenantId = _nextTenantId++;
        private readonly IEqualityComparer<DateTime?> _tolerantDateTimeComparer = new TolerantDateTimeComparer(TimeSpan.FromMilliseconds(5000));
        private readonly IEntityIdGenerator _idGenerator = A.Fake<IEntityIdGenerator>();
        private readonly DatabaseFixture _fixture;
        private readonly IClock _clock = new FrozenClock();
        

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
            using (DbSession dbs = _fixture.UseDbSession())
            {
                
                {
                    int count = dbs.Connection.ExecuteScalar<int>("SELECT count(*) FROM Blogs");
                    Assert.Equal(0, count);

                    count = dbs.Connection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                    Assert.Equal(0, count);
                }

                {
                    IUnitOfWork uow = dbs.BeginUnitOfWork(clock:_clock);
                    var sut = new EfRepository<Blog>(uow.GetDbContext(), new BlogMapping(), CurrentTenantIdHolder.Create(_tenantId), new AllowAll<Blog>());
                    var blog = new Blog(_idGenerator.NextId(), "my blog");
                    blog.AddPost(_idGenerator, "my post");
                    sut.Add(blog);
                    uow.Complete();
                }

                
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
            using (DbSession dbs = _fixture.UseDbSession())
            {
                var id = CreateBlogWithPost(dbs);
                Blog blog;

                {
                    IUnitOfWork uow = dbs.BeginUnitOfWork(clock:_clock);
                    var sut = new EfRepository<Blog>(uow.GetDbContext(), new BlogMapping(),
                        CurrentTenantIdHolder.Create(_tenantId), new AllowAll<Blog>());
                    blog = sut.Single(id);
                    uow.Complete();
                }

                
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
            using (DbSession dbs = _fixture.UseDbSession())
            {
                var id = CreateBlogWithPost(dbs);

                {
                    IUnitOfWork uow = dbs.BeginUnitOfWork(clock:_clock);
                    var sut = new EfRepository<Blog>(uow.GetDbContext(), new BlogMapping(),
                        CurrentTenantIdHolder.Create(_tenantId), new AllowAll<Blog>());
                    Blog blog = sut.Single(id);
                    blog.Modify("modified");
                    uow.Complete();
                }

                Assert.Equal(1, dbs.Connection.ExecuteScalar<int>("SELECT count(*) FROM Blogs"));
                Assert.Equal(id, dbs.Connection.ExecuteScalar<int>("SELECT Id FROM Blogs LIMIT 1"));
                Assert.Equal("modified", dbs.Connection.ExecuteScalar<string>("SELECT Name FROM Blogs LIMIT 1"));
                Assert.Equal("modified", dbs.Connection.ExecuteScalar<string>("SELECT Name FROM Posts LIMIT 1"));
            }
        }

        [Fact]
        public void CanDelete()
        {
            using (DbSession dbs = _fixture.UseDbSession())
            {
                var id = CreateBlogWithPost(dbs);

                {
                    IUnitOfWork uow = dbs.BeginUnitOfWork(clock:_clock);
                    var sut = new EfRepository<Blog>(uow.GetDbContext(), new BlogMapping(), CurrentTenantIdHolder.Create(_tenantId), new AllowAll<Blog>());
                    Blog blog = sut.Single(id);
                    sut.Delete(blog);
                    uow.Complete();
                }

                int count = dbs.Connection.ExecuteScalar<int>("SELECT count(*) FROM Blogs");
                Assert.Equal(0, count);

                count = dbs.Connection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                Assert.Equal(0, count);
            }
        }

        [Fact]
        public void CanDeleteDependent()
        {
            using (DbSession dbs = _fixture.UseDbSession())
            {
                int id = CreateBlogWithPost(dbs, 10);

                {
                    int count = dbs.Connection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                    Assert.Equal(10, count);
                }

                {
                    IUnitOfWork uow = dbs.BeginUnitOfWork(clock:_clock);
                    var sut = new EfRepository<Blog>(uow.GetDbContext(), new BlogMapping(),
                        CurrentTenantIdHolder.Create(_tenantId), new AllowAll<Blog>());
                    Blog blog = sut.Single(id);
                    Post firstPost = blog.Posts.First();
                    firstPost.SetName("sadfasfsadf");
                    blog.Posts.Remove(firstPost);
                    uow.Complete();
                }

                {
                    var count = dbs.Connection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                    Assert.Equal(9, count);
                }
            }
        }

        [Fact]
        public void CanUpdateDependant()
        {
            using (DbSession dbs = _fixture.UseDbSession())
            {
                int id = CreateBlogWithPost(dbs, 10);
                Post post;

                {
                    IUnitOfWork uow = dbs.BeginUnitOfWork(clock:_clock);
                    var sut = new EfRepository<Blog>(uow.GetDbContext(), new BlogMapping(), CurrentTenantIdHolder.Create(_tenantId),
                        new AllowAll<Blog>());
                    Blog blog = sut.Single(id);
                    post = blog.Posts.First();
                    post.SetName("modified");
                    uow.Complete();
                }

                
                {
                    string name = dbs.Connection.ExecuteScalar<string>($"SELECT name FROM Posts where id = {post.Id}");
                    Assert.Equal("modified", name);

                    string strChangedOn = dbs.Connection.ExecuteScalar<string>($"SELECT changedon FROM Posts where id = {post.Id}");
                    DateTime changedOn = DateTime.Parse(strChangedOn);
                    Assert.Equal(_clock.UtcNow, changedOn, new TolerantDateTimeComparer(TimeSpan.FromMilliseconds(500)));
                }
            }
        }

        //FAILING!!!!
        // this shows, that ValueObjects treated as OwnedTypes are not supported very well
        //[Fact]
        //public void CanUpdateDependantValueObject()
        //{
        //    using (DbSession dbs = _fixture.UseDbSession())
        //    {
        //        int id = CreateBlogWithPost(dbs, 10);
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
        //            string culture = dbs.Connection.ExecuteScalar<string>($"SELECT TargetAudience_Culture ame FROM Posts where id = {post.Id}");
        //            Assert.Equal("es-AR", culture);

        //            string strChangedOn = dbs.Connection.ExecuteScalar<string>($"SELECT ChangedOn FROM Posts where id = {post.Id}");
        //            DateTime changedOn = DateTime.Parse(strChangedOn);
        //            Assert.Equal(_clock.UtcNow, changedOn, new TolerantDateTimeComparer(TimeSpan.FromMilliseconds(500)));
        //        }
        //    }
        //}

        [Fact]
        public void CanAddDependent()
        {
            using (DbSession dbs = _fixture.UseDbSession())
            {
                int id = CreateBlogWithPost(dbs, 10);

                {
                    IUnitOfWork uow = dbs.BeginUnitOfWork(clock:_clock);
                    var sut = new EfRepository<Blog>(uow.GetDbContext(), new BlogMapping(), CurrentTenantIdHolder.Create(_tenantId), new AllowAll<Blog>());
                    Blog blog = sut.Single(id);
                    blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "added"));
                    uow.Complete();
                
                    int count = dbs.Connection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                    Assert.Equal(11, count);
                }
            }
        }


        [Fact]
        public void CanReplaceDependentCollection()
        {
            using (DbSession dbs = _fixture.UseDbSession())
            {
                var id = CreateBlogWithPost(dbs, 10);

                {
                    IUnitOfWork uow = dbs.BeginUnitOfWork(clock:_clock);
                    var sut = new EfRepository<Blog>(uow.GetDbContext(), new BlogMapping(), CurrentTenantIdHolder.Create(_tenantId), new AllowAll<Blog>());
                    Blog blog = sut.Single(id);
                    blog.Posts.Clear();
                    blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "new name 1"));
                    blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "new name 2"));
                    blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "new name 3"));
                    blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "new name 4"));
                    blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "new name 5"));
                    uow.Complete();
                }

                
                {
                    int count = dbs.Connection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                    Assert.Equal(5, count);
                }
            }
        }

        [Fact]
        public void UpdatesAggregateTrackingPropertiesOnDeleteOfDependant()
        {
            using (DbSession dbs = _fixture.UseDbSession())
            {
                int id = CreateBlogWithPost(dbs, 10);

                DateTime expectedModifiedOn = _clock.UtcNow.AddHours(1);
                _clock.OverrideUtcNow(expectedModifiedOn);

                {
                    IUnitOfWork uow = dbs.BeginUnitOfWork(clock:_clock);
                    var sut = new EfRepository<Blog>(uow.GetDbContext(), new BlogMapping(), CurrentTenantIdHolder.Create(_tenantId), new AllowAll<Blog>());
                    Blog b = sut.Single(id);
                    b.Posts.Remove(b.Posts.First());
                    uow.Complete();
                }

                {
                    IUnitOfWork uow = dbs.BeginUnitOfWork(clock:_clock);
                    Blog blog = uow.GetDbContext().Set<Blog>().Find(id);
                    Assert.NotNull(blog.ChangedOn);
                    Assert.Equal(expectedModifiedOn, blog.ChangedOn.Value, _tolerantDateTimeComparer);
                }
            }
        }

        private int CreateBlogWithPost(DbSession dbs, int postCount = 1)
        {
            
            {
                int blogId = _nextId++;
                dbs.Connection.ExecuteNonQuery(
                    $"INSERT INTO Blogs (Id, TenantId, Name, CreatedOn, CreatedBy) VALUES ({blogId}, {CurrentTenantIdHolder.Create(_tenantId).Current.Value}, 'my blog', CURRENT_TIMESTAMP, 'persistence test')");
                int count = dbs.Connection.ExecuteScalar<int>("SELECT count(*) FROM Blogs");
                Assert.Equal(1, count);

                for (int i = 0; i < postCount; i++)
                {
                    dbs.Connection.ExecuteNonQuery(
                        $"INSERT INTO Posts (Id, BlogId, Name, TargetAudience_IsPublic, TargetAudience_Culture, CreatedOn, CreatedBy) VALUES ({_nextId++}, {blogId}, 'my post {i:00}', '1', 'de-DE', CURRENT_TIMESTAMP, 'persistence test')");
                }

                return blogId;
            }
        }
    }
}
