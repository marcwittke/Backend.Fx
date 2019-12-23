using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain;
using Backend.Fx.Environment.MultiTenancy;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence
{
    public sealed class TestDbContext : DbContext
    {
        public TestDbContext([NotNull] DbContextOptions<TestDbContext> options) : base(options)
        {
            Database.AutoTransactionsEnabled = false;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            this.ApplyAggregateMappings(modelBuilder);
            modelBuilder.RegisterRowVersionProperty();
            modelBuilder.RegisterEntityIdAsNeverGenerated();
        }
        
        public DbSet<Blogger> Bloggers { get; [UsedImplicitly] set; }

        public DbSet<Blog> Blogs { get; [UsedImplicitly] set; }
        public DbSet<Post> Posts { get; [UsedImplicitly] set; }
        public DbSet<Tenant> Tenants { get; [UsedImplicitly] set; }
    }
}
