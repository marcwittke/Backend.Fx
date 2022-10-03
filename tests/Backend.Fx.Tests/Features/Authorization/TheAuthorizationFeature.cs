using System;
using System.Threading.Tasks;
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
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Features.Authorization;

public class TheAuthorizationFeature : TestWithLogging
{
    private readonly IBackendFxApplication _sut;
    private readonly IExceptionLogger _exceptionLogger = A.Fake<IExceptionLogger>();
    private readonly DummyServicesFeature _dummyServicesFeature = new ();

    public TheAuthorizationFeature(ITestOutputHelper output) : base(output)
    {
        _sut = new BackendFxApplication(new SimpleInjectorCompositionRoot(), _exceptionLogger, GetType().Assembly);
        _sut.EnableFeature(_dummyServicesFeature);
    }

    [Fact]
    public void CanNotEnableFeatureWithoutPersistence()
    {
        Assert.Throws<InvalidOperationException>(() => _sut.EnableFeature(new AuthorizationFeature()));
    }

    [Fact]
    public async Task CanAllowRead()
    {
        await ConfigureAndBootApplication();

        FillStore();

        A.CallTo(() => _dummyServicesFeature.Spies.DummyAuthorizationPolicySpy.HasAccessExpression)
            .Returns(agg => true);

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
        await ConfigureAndBootApplication();

        FillStore();

        A.CallTo(() => _dummyServicesFeature.Spies.DummyAuthorizationPolicySpy.HasAccessExpression)
            .Returns(agg => false);

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

            await Assert.ThrowsAsync<NotFoundException<DummyAggregate>>(async () =>
                await repository.ResolveAsync(new[] { 1, 2, 3, 4 }));
        }, new AnonymousIdentity());
    }

    [Fact]
    public async Task CanAllowCreate()
    {
        await ConfigureAndBootApplication();

        FillStore();

        A.CallTo(() => _dummyServicesFeature.Spies.DummyAuthorizationPolicySpy.HasAccessExpression).Returns(agg => true);
        A.CallTo(() => _dummyServicesFeature.Spies.DummyAuthorizationPolicySpy.CanCreate(A<DummyAggregate>._)).Returns(true);

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
        await ConfigureAndBootApplication();

        FillStore();

        A.CallTo(() => _dummyServicesFeature.Spies.DummyAuthorizationPolicySpy.HasAccessExpression).Returns(agg => true);
        A.CallTo(() => _dummyServicesFeature.Spies.DummyAuthorizationPolicySpy.CanCreate(A<DummyAggregate>._)).Returns(true);
        A.CallTo(() => _dummyServicesFeature.Spies.DummyAuthorizationPolicySpy.CanDelete(A<DummyAggregate>._)).Returns(true);

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
        await ConfigureAndBootApplication();

        FillStore();

        A.CallTo(() => _dummyServicesFeature.Spies.DummyAuthorizationPolicySpy.HasAccessExpression).Returns(agg => true);
        A.CallTo(() => _dummyServicesFeature.Spies.DummyAuthorizationPolicySpy.CanCreate(A<DummyAggregate>._)).Returns(false);

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
        await ConfigureAndBootApplication();

        FillStore();

        A.CallTo(() => _dummyServicesFeature.Spies.DummyAuthorizationPolicySpy.HasAccessExpression).Returns(agg => true);
        A.CallTo(() => _dummyServicesFeature.Spies.DummyAuthorizationPolicySpy.CanCreate(A<DummyAggregate>._)).Returns(true);
        A.CallTo(() => _dummyServicesFeature.Spies.DummyAuthorizationPolicySpy.CanDelete(A<DummyAggregate>._)).Returns(false);

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

    private async Task ConfigureAndBootApplication()
    {
        _sut.EnableFeature(new PersistenceFeature(new InMemoryPersistenceModule<int>()));
        _sut.EnableFeature(new AuthorizationFeature());
        await _sut.BootAsync();
    }

    private void FillStore()
    {
        var inMemoryDatabase = _sut.CompositionRoot.ServiceProvider.GetRequiredService<InMemoryDatabase<int>>();
        var dummyAggregateStore = inMemoryDatabase.GetInMemoryStores().For<DummyAggregate>();
        dummyAggregateStore.Add(1, new DummyAggregate(1, "one"));
        dummyAggregateStore.Add(2, new DummyAggregate(2, "two"));
        dummyAggregateStore.Add(3, new DummyAggregate(3, "three"));
        dummyAggregateStore.Add(4, new DummyAggregate(4, "four"));
    }
}