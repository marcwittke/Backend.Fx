using Backend.Fx.EfCore6Persistence.Tests.DummyAggregates;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCore6Persistence.Tests.DummyPersistence;

public class DummyDbContext : DbContext
{
    public DummyDbContext(DbContextOptions<DummyDbContext> options) : base(options)
    {
    }

    public DbSet<Supplier> Suppliers { get; set; }

    public DbSet<Article> Articles { get; set; }
    public DbSet<Article2> Articles2 { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DummyDbContext).Assembly);
    }
}