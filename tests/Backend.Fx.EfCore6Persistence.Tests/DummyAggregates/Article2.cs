
using System;
using System.Collections.Generic;
using Backend.Fx.Domain;
using Backend.Fx.Features.IdGeneration;
using JetBrains.Annotations;

namespace Backend.Fx.EfCore6Persistence.Tests.DummyAggregates;


/// <summary>
/// An aggregate with nested collection that uses value type
/// </summary>
public sealed class Article2 : Identified<int>, IAggregateRoot<int>
{
    private readonly ISet<ArticleVariant2> _variants = new SortedSet<ArticleVariant2>(Comparer<ArticleVariant2>.Create((left, right) => left.Id.CompareTo(right.Id)));

    [UsedImplicitly]
    private Article2()
    { }
    
    public Article2(IEntityIdGenerator<int> idGen, string sku, string name, IEnumerable<ArticleVariant2> variants)
        : base(idGen.NextId())
    {
        Sku = sku;
        Name = name;
        foreach (ArticleVariant2 variant in variants)
        {
            variant.Id = idGen.NextId();
            _variants.Add(variant);
        }
    }
    
    public string Sku { get; [UsedImplicitly] private set; }

    public string Name { get; [UsedImplicitly] private set; }

    public IEnumerable<ArticleVariant2> Variants => _variants;
    
    public static Article2 CreateNewArticle(IEntityIdGenerator<int> entityIdGenerator)
    {
        var random = new Random();
        var article = new Article2(
            entityIdGenerator,
            $"SKU-{random.Next(10000, 99999)}",
            "Article {random.Next(10000,99999)}",
            new[]
            {
                new ArticleVariant2("Green", "XL"),
                new ArticleVariant2("Blue", "S"),
                new ArticleVariant2("Black", "M"),
            });

        return article;
    }

    public void ReplaceVariants(IEntityIdGenerator<int> idGen, ArticleVariant2[] variants)
    {
        _variants.Clear();
        AddVariants(idGen, variants);
        
    }

    public void AddVariants(IEntityIdGenerator<int> idGen, ArticleVariant2[] variants)
    {
        foreach (ArticleVariant2 variant in variants)
        {
            variant.Id = idGen.NextId();
            _variants.Add(variant);
        }
    }

    public void RemoveVariant(ArticleVariant2 variant)
    {
        _variants.Remove(variant);
    }
}

public class ArticleVariant2
{
    [UsedImplicitly] 
    private ArticleVariant2()
    { }
    
    public ArticleVariant2(string color, string size)
    {
        ColorAndSize = new ColorAndSize(color, size);
    }
        
    public int Id { get; set; }

    public ColorAndSize ColorAndSize { get; private set; }
        
}

public class ColorAndSize : ValueObject
{
    public ColorAndSize(string color, string size)
    {
        Color = color;
        Size = size;
    }

    public string Color { get; private set; }
    
    public string Size { get; private set; }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Color;
        yield return Size;
    }
}