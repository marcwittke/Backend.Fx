using System.Threading.Tasks;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Features.DomainEvents;
using Backend.Fx.Logging;
using Backend.Fx.MicrosoftDependencyInjection;
using Backend.Fx.Tests.DummyServices;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Features.DomainEvents;

public class TheDomainEventsFeature : TestWithLogging
{
    private readonly IBackendFxApplication _sut;
    private readonly IExceptionLogger _exceptionLogger = A.Fake<IExceptionLogger>();
    private readonly IDummyDomainEventHandlerSpy _spy = A.Fake<IDummyDomainEventHandlerSpy>();
    
    public TheDomainEventsFeature(ITestOutputHelper output) : base(output)
    {
        _sut = new BackendFxApplication(new MicrosoftCompositionRoot(), _exceptionLogger, GetType().Assembly);
        _sut.EnableFeature(new DomainEventsFeature());
        _sut.CompositionRoot.Register(ServiceDescriptor.Singleton(_spy));
    }

    [Fact]
    public async Task InjectsDomainEventAggregator()
    {
        await _sut.BootAsync();
        await _sut.Invoker.InvokeAsync(sp =>
        {
            var domainEventAggregator = sp.GetRequiredService<IDomainEventAggregator>();
            Assert.IsType<DomainEventAggregator>(domainEventAggregator);
            return Task.CompletedTask;
        }, new AnonymousIdentity());
    }
    
    [Fact]
    public async Task InjectsDomainEventHandlers()
    {
        await _sut.BootAsync();
        
        var dummyDomainEvent = new DummyDomainEvent();
        
        await _sut.Invoker.InvokeAsync(sp =>
        {
            var domainEventHandler = sp.GetRequiredService<IDomainEventHandler<DummyDomainEvent>>();
            domainEventHandler.Handle(dummyDomainEvent);
            return Task.CompletedTask;
        }, new AnonymousIdentity());

        A.CallTo(() => _spy.Handle(A<DummyDomainEvent>.That.IsSameAs(dummyDomainEvent))).MustHaveHappenedOnceExactly();
    }
    
    [Fact]
    public async Task RaisesAndHandlesDomainEvents()
    {
        await _sut.BootAsync();
        
        var dummyDomainEvent = new DummyDomainEvent();
        await _sut.Invoker.InvokeAsync(sp =>
        {
            var domainEventAggregator = sp.GetRequiredService<IDomainEventAggregatorScope>();
            domainEventAggregator.PublishDomainEvent(dummyDomainEvent);
            return Task.CompletedTask;
        }, new AnonymousIdentity());
        
        A.CallTo(() => _spy.Handle(A<DummyDomainEvent>.That.IsSameAs(dummyDomainEvent))).MustHaveHappenedOnceExactly();
    }
}