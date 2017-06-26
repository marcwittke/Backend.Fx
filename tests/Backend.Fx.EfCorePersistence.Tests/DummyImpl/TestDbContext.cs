namespace Backend.Fx.EfCorePersistence.Tests.DummyImpl
{
    using System;
    using Environment.MultiTenancy;
    using JetBrains.Annotations;
    using Logging;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.Extensions.DependencyInjection;

    public class TestDbContext : DbContext
    {
        public TestDbContext([NotNull] DbContextOptions options) : base(options)
        {
            IServiceProvider serviceProvider = this.GetInfrastructure<IServiceProvider>();
            var loggerFactory = serviceProvider.GetService<Microsoft.Extensions.Logging.ILoggerFactory>();
            loggerFactory.AddProvider(new BackendFxLoggerProvider());
        }

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
