using System.Linq;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain;
using Backend.Fx.EfCorePersistence.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace Backend.Fx.EfCorePersistence.Tests
{
    public class TheDbContext : IClassFixture<PersistenceFixture>
    {
        private static int _nextTenantId = 2675;
        private readonly PersistenceFixture _fixture;
        private readonly int _tenantId = _nextTenantId++;

        public TheDbContext(PersistenceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void CanClearAndReplaceDependentEntities()
        {
            using (var scope = _fixture.BeginScope())
            {
                var blog = new Blog(1, "original blog") { TenantId = _tenantId };
                blog.Posts.Add(new Post(1, blog, "new name 1"));
                blog.Posts.Add(new Post(2, blog, "new name 2"));
                blog.Posts.Add(new Post(3, blog, "new name 3"));
                blog.Posts.Add(new Post(4, blog, "new name 4"));
                blog.Posts.Add(new Post(5, blog, "new name 5"));
                scope.DbContext.Add(blog);
            }

            using (var scope = _fixture.BeginScope())
            {
                var blog = scope.DbContext.Blogs.Include(b => b.Posts).Single(b => b.Id == 1);
                blog.Posts.Clear();
                blog.Posts.Add(new Post(6, blog, "new name 6"));
                blog.Posts.Add(new Post(7, blog, "new name 7"));
                blog.Posts.Add(new Post(8, blog, "new name 8"));
                blog.Posts.Add(new Post(9, blog, "new name 9"));
                blog.Posts.Add(new Post(10, blog, "new name 10"));
            }

            using (var scope = _fixture.BeginScope())
            {
                var blog = scope.DbContext.Blogs.Include(b => b.Posts).Single(b => b.Id == 1);

                Assert.Equal(5, blog.Posts.Count);

                for (var i = 1; i <= 5; i++)
                {
                    Assert.DoesNotContain(blog.Posts, p => p.Id == i);
                }

                for (var i = 6; i <= 10; i++)
                {
                    Assert.Contains(blog.Posts, p => p.Id == i);
                }
            }
        }
    }
}
