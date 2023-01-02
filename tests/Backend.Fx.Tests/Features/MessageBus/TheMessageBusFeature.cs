using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Features.MessageBus;
using Backend.Fx.Features.MessageBus.InProc;
using Backend.Fx.Logging;
using Backend.Fx.SimpleInjectorDependencyInjection;
using Backend.Fx.Tests.DummyServices;
using Backend.Fx.TestUtil;
using Backend.Fx.Util;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Features.MessageBus;

public class TheMessageBusFeature : TestWithLogging
{
    private readonly IBackendFxApplication _sender;
    private readonly IBackendFxApplication _recipient;
    private readonly IExceptionLogger _exceptionLogger = A.Fake<IExceptionLogger>();
    private readonly DummyServicesFeature _dummyServicesFeature = new();
    private readonly InProcMessageBusChannel _inProcMessageBusChannel = new();

    public TheMessageBusFeature(ITestOutputHelper output) : base(output)
    {
        _sender = new BackendFxApplication(new SimpleInjectorCompositionRoot(), _exceptionLogger);
        _sender.EnableFeature(new MessageBusFeature(new InProcMessageBus(_inProcMessageBusChannel)));
        _sender.EnableFeature(new DummyServicesFeature());
        
        _recipient =
            new BackendFxApplication(new SimpleInjectorCompositionRoot(), _exceptionLogger, GetType().Assembly);
        _recipient.EnableFeature(new MessageBusFeature(new InProcMessageBus(_inProcMessageBusChannel)));
        _recipient.EnableFeature(_dummyServicesFeature);
    }

    [Fact]
    public async Task InjectsMessageBusScope()
    {
        await _sender.BootAsync();
        
        await _sender.Invoker.InvokeAsync(sp =>
        {
            var messageBusScope = sp.GetRequiredService<IMessageBusScope>();
            Assert.NotNull(messageBusScope);
            return Task.CompletedTask;
        });
    }
    
    [Fact]
    public async Task InjectsHandlersAsCollections()
    {
        await _recipient.BootAsync();
        await _recipient.Invoker.InvokeAsync(sp =>
        {
            var handlers = sp.GetRequiredService<IEnumerable<IIntegrationEventHandler<DummyIntegrationEvent>>>().ToArray();
            Assert.NotNull(handlers);
            Assert.IsType<DummyIntegrationEventHandler>(Assert.Single(handlers));
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task PublishesAndHandlesEvents()
    {
        // we have two applications
        await _sender.BootAsync();
        await _recipient.BootAsync();
        
        // we send an integration event on the first app
        var dummyIntegrationEvent = new DummyIntegrationEvent();
        var sendingCorrelationId = Guid.Empty;
        await _sender.Invoker.InvokeAsync(sp =>
        {
            sendingCorrelationId = sp.GetRequiredService<ICurrentTHolder<Correlation>>().Current.Id;
            
            var messageBusScope = sp.GetRequiredService<IMessageBusScope>();
            messageBusScope.Publish(dummyIntegrationEvent);
            return Task.CompletedTask;
        });

        // wait that the async processing finishes
        await _inProcMessageBusChannel.FinishHandlingAllMessagesAsync();

        // assert that the second application received the message and handled the event
        A.CallTo(() =>
                _dummyServicesFeature.Spies.DummyIntegrationEventHandlerSpy.HandleAsync(
                    A<DummyIntegrationEvent>.That.Matches(ev => ev.Id == dummyIntegrationEvent.Id && sendingCorrelationId == dummyIntegrationEvent.CorrelationId)))
            .MustHaveHappenedOnceExactly();
        
        Assert.NotEqual(Guid.Empty, sendingCorrelationId);
        Assert.NotEqual(Guid.Empty, dummyIntegrationEvent.Id);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sender.Dispose();
            _recipient.Dispose();
        }
    }
}