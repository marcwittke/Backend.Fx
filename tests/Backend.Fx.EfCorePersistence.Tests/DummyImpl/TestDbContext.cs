namespace Backend.Fx.EfCorePersistence.Tests.DummyImpl
{
    using Environment.MultiTenancy;
    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;

    public class TestDbContext : DbContext
    {
        public TestDbContext([NotNull] DbContextOptions options) : base(options)
        {
            //IServiceProvider serviceProvider = this.GetInfrastructure<IServiceProvider>();
            //var loggerFactory = serviceProvider.GetService<Microsoft.Extensions.Logging.ILoggerFactory>();
            //loggerFactory.AddProvider(new NLogLoggerProvider());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            this.ApplyAggregateRootMappings(modelBuilder);
        }
        
        public DbSet<Blogger> Bloggers { get; set; }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
    }
}
