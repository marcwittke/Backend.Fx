using Backend.Fx.EfCore6Persistence.Tests.DummyAggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Fx.EfCore6Persistence.Tests.DummyPersistence;

public class ArticleEntityTypeConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.OwnsMany(art => art.Variants).WithOwner();
    }
}