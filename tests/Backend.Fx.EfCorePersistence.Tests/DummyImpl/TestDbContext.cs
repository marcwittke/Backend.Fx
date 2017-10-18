namespace Backend.Fx.EfCorePersistence.Tests.DummyImpl
{
    using Environment.MultiTenancy;
    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;

    public class TestDbContext : DbContext
    {
        public TestDbContext([NotNull] DbContextOptions<TestDbContext> options) : base(options)
        {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            this.ApplyAggregateRootMappings(modelBuilder);
            modelBuilder.RegisterRowVersionProperty();
            modelBuilder.RegisterEntityIdAsNeverGenerated();
        }
        
        public DbSet<Blogger> Bloggers { get; set; }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
    }
}
