using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.DependencyInjection;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Features;
using Backend.Fx.Features.IdGeneration;
using Backend.Fx.Tests.ExecutionPipeline;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Tests.DummyServices;

public class DummyServicesFeature : Feature, IBootableFeature
{
    private readonly DummyServicesModule _module = new();

    public DummyServicesModule Spies => _module;

    public override void Enable(IBackendFxApplication application)
    {
        application.CompositionRoot.RegisterModules(_module);
    }

    public Task BootAsync(IBackendFxApplication application, CancellationToken cancellationToken = default)
    {
        _module.ResetSpies();
        return Task.CompletedTask;
    }
}

public class DummyServicesModule : IModule
{
    public DummyDemoDataGeneratorSpy DummyDemoDataGeneratorSpy { get; } = new();
    public DummyProductiveDataGeneratorSpy DummyProductiveDataGeneratorSpy { get; } = new();
    
    public IDummyDomainEventHandlerSpy DummyDomainEventHandlerSpy { get; } = A.Fake<IDummyDomainEventHandlerSpy>();
    public IDummyJobSpy DummyJobSpy { get; } = A.Fake<IDummyJobSpy>();
    public IOperationSpy OperationSpy { get; } = A.Fake<IOperationSpy>();

    public IDummyIntegrationEventHandlerSpy DummyIntegrationEventHandlerSpy { get; } =
        A.Fake<IDummyIntegrationEventHandlerSpy>();   
    public IEntityIdGenerator<int> EntityIdGenerator { get; } = A.Fake<IEntityIdGenerator<int>>();


    public void Register(ICompositionRoot compositionRoot)
    {
        compositionRoot.Register(ServiceDescriptor.Singleton(DummyDemoDataGeneratorSpy));
        compositionRoot.Register(ServiceDescriptor.Singleton(DummyProductiveDataGeneratorSpy));
        compositionRoot.Register(ServiceDescriptor.Singleton(DummyDomainEventHandlerSpy));
        compositionRoot.Register(ServiceDescriptor.Singleton(DummyJobSpy));
        compositionRoot.Register(ServiceDescriptor.Singleton(OperationSpy));
        compositionRoot.Register(ServiceDescriptor.Singleton(DummyIntegrationEventHandlerSpy));
        compositionRoot.RegisterDecorator(ServiceDescriptor.Scoped<IOperation, OperationSpy>());
    }

    public void ResetSpies()
    {
        Fake.ClearRecordedCalls(DummyDomainEventHandlerSpy);
        Fake.ClearRecordedCalls(DummyJobSpy);
        Fake.ClearRecordedCalls(OperationSpy);
        Fake.ClearRecordedCalls(DummyIntegrationEventHandlerSpy);
    }
}