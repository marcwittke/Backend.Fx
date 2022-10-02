using System;
using System.Threading.Tasks;
using Backend.Fx.Features.MessageBus;
using Backend.Fx.Features.MessageBus.InProc;
using Backend.Fx.Logging;
using Backend.Fx.SimpleInjectorDependencyInjection;
using Backend.Fx.Tests.DummyServices;
using Backend.Fx.TestUtil;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Features.MessageBus.InProc;

public class TheInProcMessageBusChannel :TestWithLogging
{
    private readonly IExceptionLogger _exceptionLogger = A.Fake<IExceptionLogger>();

    public TheInProcMessageBusChannel(ITestOutputHelper output) : base(output)
    { }

    [Fact]
    public async Task CanBeUsedToCommunicateBetweenTwoApplications()
    {
        var sut = new InProcMessageBusChannel();

        // note that app1 does not scan any assemblies for handlers
        var app1 = new BackendFxApplication(new SimpleInjectorCompositionRoot(), _exceptionLogger);
        var bus1 = new InProcMessageBus(sut);
        app1.EnableFeature(new MessageBusFeature(bus1));
        await app1.BootAsync();
        
        // app2 will find the DummyIntegrationEventHandler type and makes a subscription  
        var app2 = new BackendFxApplication(new SimpleInjectorCompositionRoot(), _exceptionLogger, GetType().Assembly);
        var spyInApp2 = A.Fake<IDummyIntegrationEventHandlerSpy>();
        app2.CompositionRoot.Register(ServiceDescriptor.Singleton(spyInApp2));
        var bus2 = new InProcMessageBus(sut);
        app2.EnableFeature(new MessageBusFeature(bus2));
        await app2.BootAsync();
        
        await bus1.PublishAsync(new DummyIntegrationEvent());

        // the PublishAsync task awaits for completion of the publish operation, not the handling of the event.
        // instead we have to await the channel to finish its work:
        await sut.FinishHandlingAllMessagesAsync();
        
        A.CallTo(() => _exceptionLogger.LogException(A<Exception>._)).MustNotHaveHappened();
        A.CallTo(() => spyInApp2.HandleAsync(A<DummyIntegrationEvent>._)).MustHaveHappenedOnceExactly();
    }
}