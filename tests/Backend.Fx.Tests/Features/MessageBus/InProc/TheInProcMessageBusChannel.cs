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
        var sendingApp = new BackendFxApplication(new SimpleInjectorCompositionRoot(), _exceptionLogger);
        var sendingBus = new InProcMessageBus(sut);
        sendingApp.EnableFeature(new MessageBusFeature(sendingBus));
        await sendingApp.BootAsync();
        
        
        // app2 will find the DummyIntegrationEventHandler type and makes a subscription  
        var receivingApp = new BackendFxApplication(new SimpleInjectorCompositionRoot(), _exceptionLogger, GetType().Assembly);
        var receivingAppSpy = A.Fake<IDummyIntegrationEventHandlerSpy>();
        receivingApp.CompositionRoot.Register(ServiceDescriptor.Singleton(receivingAppSpy));
        var receivingBus = new InProcMessageBus(sut);
        receivingApp.EnableFeature(new MessageBusFeature(receivingBus));
        await receivingApp.BootAsync();
        
        await sendingBus.PublishAsync(new DummyIntegrationEvent());

        // the PublishAsync task awaits for completion of the publish operation, not the handling of the event.
        // instead we have to await the channel to finish its work:
        await sut.FinishHandlingAllMessagesAsync();
        
        A.CallTo(() => _exceptionLogger.LogException(A<Exception>._)).MustNotHaveHappened();
        A.CallTo(() => receivingAppSpy.HandleAsync(A<DummyIntegrationEvent>._)).MustHaveHappenedOnceExactly();
    }
}