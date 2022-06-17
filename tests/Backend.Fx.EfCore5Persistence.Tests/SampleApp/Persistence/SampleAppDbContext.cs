using Backend.Fx.Environment.MultiTenancy;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using SampleApp.Domain;

namespace Backend.Fx.EfCore5Persistence.Tests.SampleApp.Persistence
{
    public sealed class SampleAppDbContext : DbContext
    {
        public SampleAppDbContext([NotNull] DbContextOptions<SampleAppDbContext> options) : base(options)
        {
            Database.AutoTransactionsEnabled = false;
        }

        public DbSet<Blogger> Bloggers { get; [UsedImplicitly] set; }

        public DbSet<Blog> Blogs { get; [UsedImplicitly] set; }
        public DbSet<Post> Posts { get; [UsedImplicitly] set; }
        public DbSet<Tenant> Tenants { get; [UsedImplicitly] set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            this.ApplyAggregateMappings(modelBuilder);
            modelBuilder.RegisterRowVersionProperty();
            modelBuilder.RegisterEntityIdAsNeverGenerated();
        }
    }
}