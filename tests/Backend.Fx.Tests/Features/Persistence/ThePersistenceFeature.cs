using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Features.DomainEvents;
using Backend.Fx.Features.Persistence;
using Backend.Fx.Features.Persistence.InMem;
using Backend.Fx.Logging;
using Backend.Fx.MicrosoftDependencyInjection;
using Backend.Fx.Tests.DummyServices;
using Backend.Fx.TestUtil;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Features.Persistence;

public class ThePersistenceFeature : TestWithLogging
{
    private readonly IBackendFxApplication _sut;
    private readonly IExceptionLogger _exceptionLogger = A.Fake<IExceptionLogger>();
    private readonly DummyServicesFeature _dummyServicesFeature = new();

    public ThePersistenceFeature(ITestOutputHelper output) : base(output)
    {
        _sut = new BackendFxApplication(new MicrosoftCompositionRoot(), _exceptionLogger, GetType().Assembly);
    }

    [Fact]
    public async Task WaitsForDatabaseAvailabilityOnBoot()
    {
        var fake = A.Fake<IDatabaseAvailabilityAwaiter>();
        _sut.EnableFeature(new PersistenceFeature(new InMemoryPersistenceModule(), databaseAvailabilityAwaiter: fake));
        await _sut.BootAsync();

        A.CallTo(() => fake.WaitForDatabase(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task BootstrapsDatabaseOnBoot()
    {
        var fake = A.Fake<IDatabaseBootstrapper>();
        _sut.EnableFeature(new PersistenceFeature(new InMemoryPersistenceModule(), databaseBootstrapper: fake));
        await _sut.BootAsync();

        A.CallTo(() => fake.EnsureDatabaseExistenceAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task FlushesOnOperationCompletion()
    {
        _sut.EnableFeature(new PersistenceFeature(new InMemoryPersistenceModule()));
        _sut.EnableFeature(_dummyServicesFeature);
        
        ICanFlushSpy canFlushSpy = A.Fake<ICanFlushSpy>();
        _sut.CompositionRoot.RegisterDecorator(ServiceDescriptor.Scoped<ICanFlush, CanFlushSpy>());
        _sut.CompositionRoot.Register(ServiceDescriptor.Singleton(canFlushSpy));
        await _sut.BootAsync();
        
        await _sut.Invoker.InvokeAsync(_ => Task.CompletedTask);

        A.CallTo(() => canFlushSpy.Flush()).MustHaveHappenedOnceExactly();
    }

    
    [Fact]
    public async Task FlushesOnRaisingDomainEvents()
    {
        _sut.EnableFeature(new DomainEventsFeature());
        _sut.EnableFeature(new PersistenceFeature(new InMemoryPersistenceModule()));
        _sut.EnableFeature(_dummyServicesFeature);
        
        var canFlushSpy = A.Fake<ICanFlushSpy>();
        _sut.CompositionRoot.RegisterDecorator(ServiceDescriptor.Scoped<ICanFlush, CanFlushSpy>());
        _sut.CompositionRoot.Register(ServiceDescriptor.Singleton(canFlushSpy));
        await _sut.BootAsync();
        
        await _sut.Invoker.InvokeAsync(_ => Task.CompletedTask);

        // we expect two calls to Flush(): before raising domain events and when completing the operation
        A.CallTo(() => canFlushSpy.Flush()).MustHaveHappenedTwiceExactly();
    }
    
    [Fact]
    public async Task DoesNotFlushOnInvocationError()
    {
        _sut.EnableFeature(new PersistenceFeature(new InMemoryPersistenceModule()));
        _sut.EnableFeature(_dummyServicesFeature);
        
        ICanFlushSpy canFlushSpy = A.Fake<ICanFlushSpy>();
        _sut.CompositionRoot.RegisterDecorator(ServiceDescriptor.Scoped<ICanFlush, CanFlushSpy>());
        _sut.CompositionRoot.Register(ServiceDescriptor.Singleton(canFlushSpy));
        await _sut.BootAsync();
        
        await Assert.ThrowsAsync<DivideByZeroException>(async () =>
            await _sut.Invoker.InvokeAsync(_ => throw new DivideByZeroException()));

        A.CallTo(() => canFlushSpy.Flush()).MustNotHaveHappened();
    }
   

    [Fact]
    public async Task ProvidesRepositories()
    {
        _sut.EnableFeature(new PersistenceFeature(new InMemoryPersistenceModule()));
        await _sut.BootAsync();

        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var repository = sp.GetRequiredService<IRepository<DummyAggregate, int>>();
            await repository.AddAsync(new DummyAggregate(1, "one"));
        });

        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var repository = sp.GetRequiredService<IRepository<DummyAggregate, int>>();
            DummyAggregate aggregate = await repository.GetAsync(1);
            Assert.NotNull(aggregate);
        });
    }
}