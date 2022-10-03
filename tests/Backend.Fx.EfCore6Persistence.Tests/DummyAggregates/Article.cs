using System;
using System.Collections.Generic;
using Backend.Fx.Domain;
using Backend.Fx.Features.IdGeneration;
using JetBrains.Annotations;

namespace Backend.Fx.EfCore6Persistence.Tests.DummyAggregates;

/// <summary>
/// An aggregate with nested collection
/// </summary>
public sealed class Article : Identified<int>, IAggregateRoot<int>
{
    private readonly ISet<ArticleVariant> _variants = new SortedSet<ArticleVariant>(Comparer<ArticleVariant>.Create((left, right) => left.Id.CompareTo(right.Id)));

    [UsedImplicitly]
    private Article()
    { }
    
    public Article(IIdGenerator<int> idGen, string sku, string name, IEnumerable<ArticleVariant> variants)
    : base(idGen.NextId())
    {
        Sku = sku;
        Name = name;
        foreach (ArticleVariant variant in variants)
        {
            variant.Id = idGen.NextId();
            _variants.Add(variant);
        }
    }
    
    public string Sku { get; [UsedImplicitly] private set; }

    public string Name { get; [UsedImplicitly] private set; }

    public IEnumerable<ArticleVariant> Variants => _variants;
    
    public static Article CreateNewArticle(IEntityIdGenerator<int> entityIdGenerator)
    {
        var random = new Random();
        var article = new Article(
            entityIdGenerator,
            $"SKU-{random.Next(10000, 99999)}",
            "Article {random.Next(10000,99999)}",
            new[]
            {
                new ArticleVariant("Green", "XL"),
                new ArticleVariant("Blue", "S"),
                new ArticleVariant("Black", "M"),
            });

        return article;
    }
}

public class ArticleVariant
{
    public ArticleVariant(string color, string size)
    {
        Color = color;
        Size = size;
    }
        
    public int Id { get; set; }

    public string Color { get; private set; }
    public string Size { get; private set; }
        
}
