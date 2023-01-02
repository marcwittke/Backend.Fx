using Backend.Fx.EfCore6Persistence.Tests.DummyAggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Fx.EfCore6Persistence.Tests.DummyPersistence;

public class SupplierEntityTypeConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.OwnsOne(s => s.PostalAddress);
    }
}