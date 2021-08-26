using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain;
using Backend.Fx.EfCorePersistence.Tests.Fixtures;
using Backend.Fx.Extensions;
using Backend.Fx.Patterns.IdGeneration;
using Dapper;
using Xunit;

namespace Backend.Fx.EfCorePersistence.Tests
{
    public class TheRepositoryOfComposedAggregate
    {
        private readonly IEqualityComparer<DateTime?> _tolerantDateTimeComparer =
            new TolerantDateTimeComparer(TimeSpan.FromMilliseconds(5000));

        private readonly IEntityIdGenerator _idGenerator;
        private readonly PersistenceFixture _fixture;

        public TheRepositoryOfComposedAggregate()
        {
            _fixture = new PersistenceFixture();
            _idGenerator = _fixture.SingletonServices.EntityIdGenerator;
        }


        private int CreateBlogWithPost(TestScopedServices scope, int postCount = 1)
        {
            var blogId = _idGenerator.NextId();
            scope.DbConnection.Execute(
                "INSERT INTO Blogs (Id, TenantId, Name, CreatedOn, CreatedBy) " +
                $"VALUES ({blogId}, {scope.TenantId.Value}, 'my blog', CURRENT_TIMESTAMP, 'persistence test')");
            var count = scope.DbConnection.ExecuteScalar<int>("SELECT count(*) FROM Blogs");
            Assert.Equal(1, count);

            for (var i = 0; i < postCount; i++)
                scope.DbConnection.Execute(
                    "INSERT INTO Posts (Id, BlogId, Name, TargetAudience_IsPublic, TargetAudience_Culture, CreatedOn, CreatedBy) " +
                    $"VALUES ({_idGenerator.NextId()}, {blogId}, 'my post {i:00}', '1', 'de-DE', CURRENT_TIMESTAMP, 'persistence test')");

            return blogId;
        }

        // //FAILING!!!!
        // // this shows, that ValueObjects treated as OwnedTypes are not supported very well
        // [Fact]
        // public void CanUpdateDependantValueObject()
        // {
        //     int id;
        //     using (var scope = _fixture.BeginScope())
        //     {
        //         id = CreateBlogWithPost(scope, 10);
        //     }
        //
        //     Post post;
        //     DateTime? expectedChangedOn;
        //     using (var scope = _fixture.BeginScope())
        //     {
        //         expectedChangedOn = _fixture.SingletonServices.Clock.UtcNow;
        //         post = scope.GetRepository<Blog>().Single(id).Posts.First();
        //         post.TargetAudience = new TargetAudience { Culture = "es-AR", IsPublic = false };
        //     }
        //
        //     using (var scope = _fixture.BeginScope())
        //     {
        //         string culture = scope.DbConnection.ExecuteScalar<string>($"SELECT TargetAudience_Culture FROM Posts where id = {post.Id}");
        //         Assert.Equal("es-AR", culture);
        //
        //         var blog = scope.GetRepository<Blog>().Single(id);
        //         
        //         string strChangedOn = scope.DbConnection.ExecuteScalar<string>($"SELECT ChangedOn FROM Posts where id = {post.Id}");
        //         DateTime changedOn = DateTime.Parse(strChangedOn);
        //         Assert.Equal(expectedChangedOn, changedOn, new TolerantDateTimeComparer(TimeSpan.FromMilliseconds(500)));
        //     }
        // }


        [Fact]
        public void CanAddDependent()
        {
            using (var scope = _fixture.BeginScope())
            {
                var id = CreateBlogWithPost(scope, 10);

                var sut = scope.GetRepository<Blog>();

                Blog blog = sut.Single(id);
                blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "added"));
            }

            using (var scope = _fixture.BeginScope())
            {
                var count = scope.DbConnection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                Assert.Equal(11, count);
            }
        }

        [Fact]
        public void CanCreate()
        {
            using (var scope = _fixture.BeginScope())
            {
                var count = scope.DbConnection.ExecuteScalar<int>("SELECT count(*) FROM Blogs");
                Assert.Equal(0, count);

                count = scope.DbConnection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                Assert.Equal(0, count);
            }

            using (var scope = _fixture.BeginScope())
            {
                var sut = scope.GetRepository<Blog>();
                var blog = new Blog(_idGenerator.NextId(), "my blog");
                blog.AddPost(_idGenerator, "my post");
                sut.Add(blog);
            }

            using (var scope = _fixture.BeginScope())
            {
                var count = scope.DbConnection.ExecuteScalar<int>("SELECT count(*) FROM Blogs");
                Assert.Equal(1, count);

                count = scope.DbConnection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                Assert.Equal(1, count);
            }
        }

        [Fact]
        public void CanDelete()
        {
            using (var scope = _fixture.BeginScope())
            {
                var id = CreateBlogWithPost(scope);

                var sut = scope.GetRepository<Blog>();
                Blog blog = sut.Single(id);
                sut.Delete(blog);
            }

            using (var scope = _fixture.BeginScope())
            {
                var count = scope.DbConnection.ExecuteScalar<int>("SELECT count(*) FROM Blogs");
                Assert.Equal(0, count);

                count = scope.DbConnection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                Assert.Equal(0, count);
            }
        }

        [Fact]
        public void CanDeleteDependent()
        {
            int id;
            using (var scope = _fixture.BeginScope())
            {
                id = CreateBlogWithPost(scope, 10);
                var count = scope.DbConnection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                Assert.Equal(10, count);
            }

            using (var scope = _fixture.BeginScope())
            {
                var sut = scope.GetRepository<Blog>();
                Blog blog = sut.Single(id);
                Post firstPost = blog.Posts.First();
                firstPost.SetName("Something different");
                blog.Posts.Remove(firstPost);
                
                scope.DbContext.TraceChangeTrackerState();
            }

            using (var scope = _fixture.BeginScope())
            {
                var count = scope.DbConnection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                Assert.Equal(9, count);
            }
        }


        [Fact]
        public void CanRead()
        {
            int id;
            Blog blog;

            using (var scope = _fixture.BeginScope())
            {
                id = CreateBlogWithPost(scope);
            }

            using (var scope = _fixture.BeginScope())
            {
                var sut = scope.GetRepository<Blog>();
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
            using (var scope = _fixture.BeginScope())
            {
                id = CreateBlogWithPost(scope, 10);
            }

            using (var scope = _fixture.BeginScope())
            {
                var sut = scope.GetRepository<Blog>();
                Blog blog = sut.Single(id);
                blog.Posts.Clear();
                blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "new name 1"));
                blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "new name 2"));
                blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "new name 3"));
                blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "new name 4"));
                blog.Posts.Add(new Post(_idGenerator.NextId(), blog, "new name 5"));
            }

            using (var scope = _fixture.BeginScope())
            {
                var count = scope.DbConnection.ExecuteScalar<int>("SELECT count(*) FROM Posts");
                Assert.Equal(5, count);
            }
        }

        [Fact]
        public void CanUpdate()
        {
            int id;
            using (var scope = _fixture.BeginScope())
            {
                id = CreateBlogWithPost(scope);
            }

            using (var scope = _fixture.BeginScope())
            {
                var sut = scope.GetRepository<Blog>();
                Blog blog = sut.Single(id);
                blog.Modify("modified");
            }

            using (var scope = _fixture.BeginScope())
            {
                Assert.Equal(1, scope.DbConnection.ExecuteScalar<int>("SELECT count(*) FROM Blogs"));
                Assert.Equal(id, scope.DbConnection.ExecuteScalar<int>("SELECT Id FROM Blogs LIMIT 1"));
                Assert.Equal("modified", scope.DbConnection.ExecuteScalar<string>("SELECT Name FROM Blogs LIMIT 1"));
                Assert.Equal("modified", scope.DbConnection.ExecuteScalar<string>("SELECT Name FROM Posts LIMIT 1"));
            }
        }

        [Fact]
        public void CanUpdateDependant()
        {
            _fixture.SingletonServices.Clock.OverrideUtcNow(new DateTime(2000, 1, 2, 11, 22, 33));
            int id;
            using (var scope = _fixture.BeginScope())
            {
                id = CreateBlogWithPost(scope, 10);
            }

            Post post;

            using (var scope = _fixture.BeginScope())
            {
                var sut = scope.GetRepository<Blog>();
                Blog blog = sut.Single(id);
                post = blog.Posts.First();
                post.SetName("modified");
            }

            using (var scope = _fixture.BeginScope())
            {
                var name = scope.DbConnection.ExecuteScalar<string>($"SELECT name FROM Posts where id = {post.Id}");
                Assert.Equal("modified", name);

                var strChangedOn = scope.DbConnection.ExecuteScalar<string>($"SELECT ChangedOn FROM Posts where id = {post.Id}");
                DateTime changedOn = DateTime.Parse(strChangedOn);
                Assert.Equal(_fixture.SingletonServices.Clock.UtcNow, changedOn,
                    new TolerantDateTimeComparer(TimeSpan.FromMilliseconds(500)));
            }
        }

        [Fact]
        public void UpdatesAggregateTrackingPropertiesOnDeleteOfDependant()
        {
            _fixture.SingletonServices.Clock.OverrideUtcNow(new DateTime(2000, 1, 2, 11, 22, 33));

            int id;
            using (var scope = _fixture.BeginScope())
            {
                id = CreateBlogWithPost(scope, 10);
            }

            DateTime expectedModifiedOn = _fixture.SingletonServices.Clock.Advance(TimeSpan.FromHours(1));

            using (var scope = _fixture.BeginScope())
            {
                var sut = scope.GetRepository<Blog>();
                Blog b = sut.Single(id);
                b.Posts.Remove(b.Posts.First());
            }

            using (var scope = _fixture.BeginScope())
            {
                Blog blog = scope.DbContext.Set<Blog>().Find(id);
                Assert.NotNull(blog.ChangedOn);
                Assert.Equal(expectedModifiedOn, blog.ChangedOn.Value, _tolerantDateTimeComparer);
            }
        }
    }
}