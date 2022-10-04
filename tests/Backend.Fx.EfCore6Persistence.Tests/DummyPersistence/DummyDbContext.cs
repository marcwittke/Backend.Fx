using Backend.Fx.EfCore6Persistence.Tests.DummyAggregates;
using Backend.Fx.Features.MultiTenancy;
using Backend.Fx.Util;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCore6Persistence.Tests.DummyPersistence;

public class DummyDbContext : MultiTenancyDbContext
{
    public DummyDbContext( ICurrentTHolder<TenantId> tenantIdHolder, DbContextOptions<DummyDbContext> options)
        : base(tenantIdHolder, options)
    {
    }

    public DbSet<Supplier> Suppliers { get; set; }

    public DbSet<Article> Articles { get; set; }
    public DbSet<Article2> Articles2 { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DummyDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
