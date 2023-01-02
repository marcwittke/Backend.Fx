using Backend.Fx.EfCore6Persistence.Tests.DummyAggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Fx.EfCore6Persistence.Tests.DummyPersistence;

public class Article2EntityTypeConfiguration : IEntityTypeConfiguration<Article2>
{
    public void Configure(EntityTypeBuilder<Article2> builder)
    {
        builder.OwnsMany(art => art.Variants, bld =>
        {
            bld.OwnsOne(v => v.ColorAndSize);
            bld.WithOwner();
        });
    }
}