using System;
using System.Threading.Tasks;
using Backend.Fx.DependencyInjection;
using Backend.Fx.Exceptions;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Features.Authorization;
using Backend.Fx.Features.Persistence;
using Backend.Fx.Features.Persistence.InMem;
using Backend.Fx.Logging;
using Backend.Fx.SimpleInjectorDependencyInjection;
using Backend.Fx.Tests.DummyServices;
using Backend.Fx.TestUtil;
using FakeItEasy;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Features.Authorization;

[UsedImplicitly]
public class TheAuthorizationFeatureWithSimpleInjector : TheAuthorizationFeature
{
    public TheAuthorizationFeatureWithSimpleInjector(ITestOutputHelper output)
        : base(new SimpleInjectorCompositionRoot(), output)
    {
    }
}

// failing: Microsoft's DI does not support decoration of open generic types. See https://github.com/khellang/Scrutor/issues/39 for more info
// [UsedImplicitly]
// public class TheAuthorizationFeatureWithMicrosoftDI : TheAuthorizationFeature
// {
//     public TheAuthorizationFeatureWithMicrosoftDI(ITestOutputHelper output) 
//         : base(new MicrosoftCompositionRoot(), output)
//     {
//     }
// }

public abstract class TheAuthorizationFeature : TestWithLogging, IAsyncLifetime
{
    private readonly IBackendFxApplication _sut;
    private readonly IExceptionLogger _exceptionLogger = A.Fake<IExceptionLogger>();
    private readonly MockFeature _mockFeature = new();
    private readonly IAuthorizationPolicy<DummyAggregate> _policyMock;

    protected TheAuthorizationFeature(ICompositionRoot compositionRoot, ITestOutputHelper output) : base(output)
    {
        _sut = new BackendFxApplication(compositionRoot, _exceptionLogger, GetType().Assembly);
        _sut.EnableFeature(_mockFeature);
        _sut.EnableFeature(new PersistenceFeature(new InMemoryPersistenceModule<int>()));
        _sut.EnableFeature(new AuthorizationFeature());

        _policyMock = _mockFeature.AddMock<IAuthorizationPolicy<DummyAggregate>>();
    }

    public async Task InitializeAsync()
    {
        await _sut.BootAsync();
    }

    [Fact]
    public void CanNotEnableFeatureWithoutPersistence()
    {
        var sutWithoutPersistence = new BackendFxApplication(new SimpleInjectorCompositionRoot(), _exceptionLogger, GetType().Assembly);
        Assert.Throws<InvalidOperationException>(() => sutWithoutPersistence.EnableFeature(new AuthorizationFeature()));
    }

    [Fact]
    public async Task CanAllowRead()
    {
        FillStore();

        A.CallTo(() => _policyMock.HasAccessExpression).Returns(agg => true);

        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var repository = sp.GetRequiredService<IRepository<DummyAggregate, int>>();
            var aggregates = await repository.GetAllAsync();
            Assert.NotEmpty(aggregates);
            Assert.Equal(4, aggregates.Length);

            var repoHasAny = await repository.AnyAsync();
            Assert.True(repoHasAny);

            for (var i = 1; i <= 4; i++)
            {
                var aggregate = await repository.FindAsync(i);
                Assert.NotNull(aggregate);

                aggregate = await repository.GetAsync(i);
                Assert.NotNull(aggregate);
            }

            aggregates = await repository.ResolveAsync(new[] { 1, 2, 3, 4 });
            Assert.NotEmpty(aggregates);
            Assert.Equal(4, aggregates.Length);
        }, new AnonymousIdentity());
    }

    [Fact]
    public async Task CanDenyRead()
    {
        FillStore();

        A.CallTo(() => _policyMock.HasAccessExpression).Returns(agg => false);

        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var repository = sp.GetRequiredService<IRepository<DummyAggregate, int>>();
            var aggregates = await repository.GetAllAsync();
            Assert.Empty(aggregates);

            var repoHasAny = await repository.AnyAsync();
            Assert.False(repoHasAny);

            for (var i = 1; i <= 4; i++)
            {
                var aggregate = await repository.FindAsync(i);
                Assert.Null(aggregate);

                await Assert.ThrowsAsync<NotFoundException<DummyAggregate>>(async () =>
                    await repository.GetAsync(i));
            }

            await Assert.ThrowsAsync<NotFoundException>(async () =>
                await repository.ResolveAsync(new[] { 1, 2, 3, 4 }));
        }, new AnonymousIdentity());
    }

    [Fact]
    public async Task CanAllowCreate()
    {
        FillStore();

        A.CallTo(() => _policyMock.HasAccessExpression).Returns(agg => true);
        A.CallTo(() => _policyMock.CanCreate(A<DummyAggregate>._)).Returns(true);

        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var repository = sp.GetRequiredService<IRepository<DummyAggregate, int>>();
            await repository.AddAsync(new DummyAggregate(1000, "thousand"));
            await repository.AddRangeAsync(new[]
            {
                new DummyAggregate(1001, "thousand and one"),
                new DummyAggregate(1002, "thousand and two")
            });
        }, new AnonymousIdentity());

        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var repository = sp.GetRequiredService<IRepository<DummyAggregate, int>>();
            var unused1 = await repository.GetAsync(1000);
            var unused2 = await repository.GetAsync(1001);
            var unused3 = await repository.GetAsync(1002);
        }, new AnonymousIdentity());
    }

    [Fact]
    public async Task CanAllowDelete()
    {
        FillStore();

        A.CallTo(() => _policyMock.HasAccessExpression).Returns(agg => true);
        A.CallTo(() => _policyMock.CanCreate(A<DummyAggregate>._)).Returns(true);
        A.CallTo(() => _policyMock.CanDelete(A<DummyAggregate>._)).Returns(true);

        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var repository = sp.GetRequiredService<IRepository<DummyAggregate, int>>();
            await repository.AddAsync(new DummyAggregate(1000, "thousand"));
        }, new AnonymousIdentity());

        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var repository = sp.GetRequiredService<IRepository<DummyAggregate, int>>();
            var aggregate = await repository.GetAsync(1000);
            await repository.DeleteAsync(aggregate);
        }, new AnonymousIdentity());

        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var repository = sp.GetRequiredService<IRepository<DummyAggregate, int>>();
            var aggregate = await repository.FindAsync(1000);
            Assert.Null(aggregate);
        }, new AnonymousIdentity());
    }

    [Fact]
    public async Task CanDenyCreate()
    {
        FillStore();

        A.CallTo(() => _policyMock.HasAccessExpression).Returns(agg => true);
        A.CallTo(() => _policyMock.CanCreate(A<DummyAggregate>._)).Returns(false);

        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var repository = sp.GetRequiredService<IRepository<DummyAggregate, int>>();
            await Assert.ThrowsAsync<ForbiddenException>(async () =>
                await repository.AddAsync(new DummyAggregate(1000, "thousand")));
            await Assert.ThrowsAsync<ForbiddenException>(async () => await repository.AddRangeAsync(
                new[]
                {
                    new DummyAggregate(1001, "thousand and one"),
                    new DummyAggregate(1002, "thousand and two")
                }
            ));
        }, new AnonymousIdentity());
    }

    [Fact]
    public async Task CanDenyDelete()
    {
        FillStore();

        A.CallTo(() => _policyMock.HasAccessExpression).Returns(agg => true);
        A.CallTo(() => _policyMock.CanCreate(A<DummyAggregate>._)).Returns(true);
        A.CallTo(() => _policyMock.CanDelete(A<DummyAggregate>._)).Returns(false);

        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var repository = sp.GetRequiredService<IRepository<DummyAggregate, int>>();
            await repository.AddAsync(new DummyAggregate(1000, "thousand"));
        }, new AnonymousIdentity());

        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var repository = sp.GetRequiredService<IRepository<DummyAggregate, int>>();
            var aggregate = await repository.GetAsync(1000);
            await Assert.ThrowsAsync<ForbiddenException>(async () => await repository.DeleteAsync(aggregate));
        }, new AnonymousIdentity());
    }

    private void FillStore()
    {
        var inMemoryDatabase = _sut.CompositionRoot.ServiceProvider.GetRequiredService<InMemoryDatabase>();
        var dummyAggregateStore = inMemoryDatabase.GetInMemoryStores().For<DummyAggregate, int>();
        dummyAggregateStore.Add(1, new DummyAggregate(1, "one"));
        dummyAggregateStore.Add(2, new DummyAggregate(2, "two"));
        dummyAggregateStore.Add(3, new DummyAggregate(3, "three"));
        dummyAggregateStore.Add(4, new DummyAggregate(4, "four"));
    }

    public Task DisposeAsync()

    {
        return Task.CompletedTask;
    }
}