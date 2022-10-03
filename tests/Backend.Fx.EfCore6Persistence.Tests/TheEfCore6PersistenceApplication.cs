using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Backend.Fx.EfCore6Persistence.Tests.DummyAggregates;
using Backend.Fx.EfCore6Persistence.Tests.DummyPersistence;
using Backend.Fx.EfCore6Persistence.Tests.Fixtures;
using Backend.Fx.Features.IdGeneration;
using Backend.Fx.Features.IdGeneration.InMem;
using Backend.Fx.Features.Persistence;
using Backend.Fx.Logging;
using Backend.Fx.SimpleInjectorDependencyInjection;
using Backend.Fx.TestUtil;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.EfCore6Persistence.Tests;

public class TheEfCore6PersistenceApplication : TestWithLogging
{
    private readonly TestDatabase _fixture = new SqliteTestDatabase();
    private readonly IBackendFxApplication _sut;
    private readonly IExceptionLogger _exceptionLogger = A.Fake<IExceptionLogger>();

    private readonly IEntityIdGenerator<int> _entityIdGenerator =
        new EntityIdGenerator<int>(new SequenceHiLoIntIdGenerator(new InMemorySequence(1000)));

    private readonly DummyDbContextOptionsFactory _dummyDbContextOptionsFactory;

    public TheEfCore6PersistenceApplication(ITestOutputHelper output) : base(output)
    {
        _sut = new BackendFxApplication(new SimpleInjectorCompositionRoot(), _exceptionLogger, GetType().Assembly);
        _sut.EnableFeature(new IdGenerationFeature<int>(_entityIdGenerator));

        _dummyDbContextOptionsFactory = new DummyDbContextOptionsFactory();
        _sut.EnableFeature(new PersistenceFeature(
            new EfCorePersistenceModule<DummyDbContext>(_fixture, _dummyDbContextOptionsFactory),
            databaseBootstrapper: new DummyDatabaseBootstrapper(_fixture, _dummyDbContextOptionsFactory)));
    }

    [Fact]
    public async Task CreatesDatabaseOnBoot()
    {
        await _sut.BootAsync();

        await using var dbContext =
            new DummyDbContext(_dummyDbContextOptionsFactory.GetDbContextOptions(_fixture.Create()));

        var unused = dbContext.Suppliers.ToArray();
    }

    [Fact]
    public async Task InjectsScopedDbContext()
    {
        await _sut.BootAsync();

        int hashCode = 0;
        await _sut.Invoker.InvokeAsync(sp =>
        {
            var dbContext1 = sp.GetRequiredService<DbContext>();
            var dbContext2 = sp.GetRequiredService<DbContext>();

            Assert.NotNull(dbContext1);
            Assert.StrictEqual(dbContext1, dbContext2);

            hashCode = dbContext1.GetHashCode();

            return Task.CompletedTask;
        });

        await _sut.Invoker.InvokeAsync(sp =>
        {
            var dbContext3 = sp.GetRequiredService<DbContext>();

            Assert.NotEqual(hashCode, dbContext3.GetHashCode());

            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task InjectsScopedRepositories()
    {
        await _sut.BootAsync();

        int hashCode = 0;
        await _sut.Invoker.InvokeAsync(sp =>
        {
            var repo1 = sp.GetRequiredService<IRepository<Supplier, int>>();
            var repo2 = sp.GetRequiredService<IRepository<Supplier, int>>();

            Assert.NotNull(repo1);
            Assert.StrictEqual(repo1, repo2);

            hashCode = repo1.GetHashCode();

            return Task.CompletedTask;
        });


        await _sut.Invoker.InvokeAsync(sp =>
        {
            var repo3 = sp.GetRequiredService<IRepository<Supplier, int>>();

            Assert.NotEqual(hashCode, repo3.GetHashCode());

            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task SavesAggregateWithOwnedValueType()
    {
        await _sut.BootAsync();

        Supplier supplier = Supplier.CreateNewSupplier(_entityIdGenerator);

        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var repo = sp.GetRequiredService<IRepository<Supplier, int>>();
            await repo.AddAsync(supplier);
        });

        Supplier loadedSupplier = null;
        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var dbContext = sp.GetRequiredService<DbContext>();
            loadedSupplier = await dbContext.Set<Supplier>().SingleAsync(o => o.Id == supplier.Id);
        });

        Assert.NotNull(loadedSupplier);
        Assert.Equal(supplier, loadedSupplier);
        Assert.Equal(supplier.Name, loadedSupplier.Name);
        Assert.Equal(supplier.PostalAddress, loadedSupplier.PostalAddress);
    }

    [Fact]
    public async Task SavesAggregateWithNestedCollection()
    {
        await _sut.BootAsync();

        Article article = Article.CreateNewArticle(_entityIdGenerator);

        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var repo = sp.GetRequiredService<IRepository<Article, int>>();
            await repo.AddAsync(article);
        });

        Article loadedArticle = null;
        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var dbContext = sp.GetRequiredService<DbContext>();
            loadedArticle = await dbContext.Set<Article>().SingleAsync(o => o.Id == article.Id);
        });

        Assert.NotNull(loadedArticle);
        AssertEqual(article, loadedArticle);
    }

    [Fact]
    public async Task SavesAggregateWithNestedCollectionThatUsesOwnedType()
    {
        await _sut.BootAsync();

        Article2 article = Article2.CreateNewArticle(_entityIdGenerator);

        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var repo = sp.GetRequiredService<IRepository<Article2, int>>();
            await repo.AddAsync(article);
        });

        Article2 loadedArticle = null;
        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var dbContext = sp.GetRequiredService<DbContext>();
            loadedArticle = await dbContext.Set<Article2>().SingleAsync(o => o.Id == article.Id);
        });

        Assert.NotNull(loadedArticle);
        AssertEqual(article, loadedArticle);
    }

    [Fact]
    public async Task CanReplaceNestedCollectionCompletely()
    {
        await _sut.BootAsync();

        Article2 article = Article2.CreateNewArticle(_entityIdGenerator);

        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var repo = sp.GetRequiredService<IRepository<Article2, int>>();
            await repo.AddAsync(article);
        });

        Article2 updatedArticle = null;
        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var dbContext = sp.GetRequiredService<DbContext>();
            updatedArticle = await dbContext.Set<Article2>().SingleAsync(o => o.Id == article.Id);
            updatedArticle.ReplaceVariants(
                _entityIdGenerator,
                new[]
                {
                    new ArticleVariant2("yellow", "XXL"),
                    new ArticleVariant2("orange", "XXL")
                });
        });

        Article2 loadedArticle = null;
        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var dbContext = sp.GetRequiredService<DbContext>();
            loadedArticle = await dbContext.Set<Article2>().SingleAsync(o => o.Id == article.Id);
        });

        AssertEqual(updatedArticle, loadedArticle);
    }

    [Fact]
    public async Task CanAddToNestedCollection()
    {
        await _sut.BootAsync();

        Article2 article = Article2.CreateNewArticle(_entityIdGenerator);
        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var repo = sp.GetRequiredService<IRepository<Article2, int>>();
            await repo.AddAsync(article);
        });

        Article2 updatedArticle = null;
        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var dbContext = sp.GetRequiredService<DbContext>();
            updatedArticle = await dbContext.Set<Article2>().SingleAsync(o => o.Id == article.Id);

            updatedArticle.AddVariants(_entityIdGenerator, new[]
            {
                new ArticleVariant2("yellow", "XXL"),
                new ArticleVariant2("orange", "XXL")
            });
        });
        
        Article2 loadedArticle = null;
        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var dbContext = sp.GetRequiredService<DbContext>();
            loadedArticle = await dbContext.Set<Article2>().SingleAsync(o => o.Id == article.Id);
        });

        AssertEqual(updatedArticle, loadedArticle);
    }

    [Fact]
    public async Task CanRemoveFromNestedCollection()
    {
        await _sut.BootAsync();

        Article2 article = Article2.CreateNewArticle(_entityIdGenerator);
        
        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var repo = sp.GetRequiredService<IRepository<Article2, int>>();
            await repo.AddAsync(article);
        });

        Article2 updatedArticle = null;
        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var dbContext = sp.GetRequiredService<DbContext>();
            updatedArticle = await dbContext.Set<Article2>().SingleAsync(o => o.Id == article.Id);
            updatedArticle.RemoveVariant(updatedArticle.Variants.First());
        });
        
        Article2 loadedArticle = null;
        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var dbContext = sp.GetRequiredService<DbContext>();
            loadedArticle = await dbContext.Set<Article2>().SingleAsync(o => o.Id == article.Id);
        });

        AssertEqual(updatedArticle, loadedArticle);
    }

    private static void AssertEqual(Article left, Article right)
    {
        var leftSerialized = JsonSerializer.Serialize(left);
        var rightSerialized = JsonSerializer.Serialize(right);
        Assert.Equal(leftSerialized, rightSerialized);
    }

    private static void AssertEqual(Article2 left, Article2 right)
    {
        var leftSerialized = JsonSerializer.Serialize(left);
        var rightSerialized = JsonSerializer.Serialize(right);
        Assert.Equal(leftSerialized, rightSerialized);
    }
}