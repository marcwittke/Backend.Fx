using System;
using System.Threading.Tasks;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Features.MessageBus;
using Backend.Fx.Features.MessageBus.InProc;
using Backend.Fx.Features.MultiTenancy;
using Backend.Fx.Features.MultiTenancy.InProc;
using Backend.Fx.Features.MultiTenancyAdmin;
using Backend.Fx.Features.MultiTenancyAdmin.InMem;
using Backend.Fx.Logging;
using Backend.Fx.SimpleInjectorDependencyInjection;
using Backend.Fx.Tests.DummyServices;
using Backend.Fx.TestUtil;
using Backend.Fx.Util;
using FakeItEasy;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Features.MessageBus;

public class TheMessageBusFeatureWithMultiTenancy : TestWithLogging
{
    private readonly InMemoryTenantRepository _tenantRepository = new();
    private readonly IBackendFxApplication _sender;
    private readonly IBackendFxApplication _recipient;
    private readonly IExceptionLogger _exceptionLogger = A.Fake<IExceptionLogger>();
    private readonly DummyServicesFeature _dummyServicesFeature = new();
    private readonly InProcMessageBusChannel _inProcMessageBusChannel = new();

    public TheMessageBusFeatureWithMultiTenancy(ITestOutputHelper output) : base(output)
    {
        _tenantRepository.SaveTenant(new Tenant(1, "t1", "tenant 1", false));
        _tenantRepository.SaveTenant(new Tenant(2, "t2", "tenant 2", true));

        _sender = new MultiTenancyBackendFxApplication<DummyTenantIdSelector>(
            new SimpleInjectorCompositionRoot(),
            _exceptionLogger, 
            new DirectTenantEnumerator(_tenantRepository),
            new InProcTenantWideMutexManager());
        _sender.EnableFeature(new MessageBusFeature(new InProcMessageBus(_inProcMessageBusChannel)));
        _sender.EnableFeature(new DummyServicesFeature());

        // for this test we do not provide any tenantId, so that we can check whether the message handling detects the correct tenant id 
        _recipient = new MultiTenancyBackendFxApplication<NullTenantIdSelector>(
            new SimpleInjectorCompositionRoot(),
            _exceptionLogger,
            new DirectTenantEnumerator(_tenantRepository),
            new InProcTenantWideMutexManager(),
            GetType().Assembly);
        _recipient.EnableFeature(new MessageBusFeature(new InProcMessageBus(_inProcMessageBusChannel)));
        _recipient.EnableFeature(_dummyServicesFeature);

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
        int? sendingTenantId = null;
        
        
        await _sender.Invoker.InvokeAsync(sp =>
        {
            sendingTenantId = sp.GetRequiredService<ICurrentTHolder<TenantId>>().Current.Value;
            sendingCorrelationId = sp.GetRequiredService<ICurrentTHolder<Correlation>>().Current.Id;
            var messageBusScope = sp.GetRequiredService<IMessageBusScope>();
            messageBusScope.Publish(dummyIntegrationEvent);
            return Task.CompletedTask;
        });

        Assert.True(dummyIntegrationEvent.Properties.ContainsKey("TenantId"));
        Assert.NotNull(sendingTenantId);
        Assert.Equal(sendingTenantId!.Value.ToString(), dummyIntegrationEvent.Properties["TenantId"]);
        
        // wait that the async processing finishes
        await _inProcMessageBusChannel.FinishHandlingAllMessagesAsync();

        A.CallTo(()=>_exceptionLogger.LogException(A<Exception>._)).MustNotHaveHappened();
        
        // assert that the second application received the message and handled the event
        A.CallTo(() =>
                _dummyServicesFeature.Spies.DummyIntegrationEventHandlerSpy.HandleAsync(
                    A<DummyIntegrationEvent>.That.Matches(ev => ev.Id == dummyIntegrationEvent.Id 
                                                                && sendingCorrelationId == dummyIntegrationEvent.CorrelationId
                                                                && sendingTenantId.Value.ToString() == dummyIntegrationEvent.Properties["TenantId"]
                                                                )))
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

    [UsedImplicitly]
    private class DummyTenantIdSelector : ICurrentTenantIdSelector
    {
        public static int? TenantId = 1000;

        public TenantId GetCurrentTenantId()
        {
            return new TenantId(TenantId);
        }
    }
    
    [UsedImplicitly]
    private class NullTenantIdSelector : ICurrentTenantIdSelector
    {
        public static int? TenantId = null;

        public TenantId GetCurrentTenantId()
        {
            return new TenantId(TenantId);
        }
    }
}