using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain;
using Backend.Fx.Environment.MultiTenancy;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence
{
    public class TestDbContext : DbContext
    {
        public TestDbContext([NotNull] DbContextOptions<TestDbContext> options) : base(options)
        {}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            Database.AutoTransactionsEnabled = false;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            this.ApplyAggregateMappings(modelBuilder);
            modelBuilder.RegisterRowVersionProperty();
            modelBuilder.RegisterEntityIdAsNeverGenerated();
        }
        
        public DbSet<Blogger> Bloggers { get; set; }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
    }
}
