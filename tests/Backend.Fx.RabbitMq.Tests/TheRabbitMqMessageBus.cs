using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Backend.Fx.Features.MessageBus;
using Backend.Fx.Logging;
using Backend.Fx.SimpleInjectorDependencyInjection;
using Backend.Fx.TestUtil;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.RabbitMq.Tests;

public class TheRabbitMqMessageBus : TestWithLogging
{
    private readonly IBackendFxApplication _sender;
    private readonly IBackendFxApplication _recipient;
    private readonly IExceptionLogger _exceptionLogger = A.Fake<IExceptionLogger>();

    // requires rabbitmq running on localhost:
    // docker run -p 5672:5672 -e RABBITMQ_DEFAULT_USER=test -e RABBITMQ_DEFAULT_PASS=password rabbitmq
    private readonly RabbitMqOptions _options = new()
    {
        Hostname = "localhost",
        Username = "test",
        Password = "password",
        ExchangeName = "test-exchange",
        ReceiveQueueName = "test-queue",
        RetryCount = 3,
    };

    public TheRabbitMqMessageBus(ITestOutputHelper output) : base(output)
    {
        _sender = new BackendFxApplication(new SimpleInjectorCompositionRoot(), _exceptionLogger);
        _sender.EnableFeature(new MessageBusFeature(new RabbitMqMessageBus(_options)));
        
        _recipient = new BackendFxApplication(new SimpleInjectorCompositionRoot(), _exceptionLogger, GetType().Assembly);
        _recipient.EnableFeature(new MessageBusFeature(new RabbitMqMessageBus(_options)));
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

    //todo fixme [Fact]
    public async Task PublishesAndHandlesEvents()
    {
        // we have two applications
        await _sender.BootAsync();
        await _recipient.BootAsync();
        
        // we send an integration event on the first app
        var dummyIntegrationEvent = new DummyIntegrationEvent();
        await _sender.Invoker.InvokeAsync(sp =>
        {
            var messageBusScope = sp.GetRequiredService<IMessageBusScope>();
            messageBusScope.Publish(dummyIntegrationEvent);
            return Task.CompletedTask;
        });

        Assert.True(DummyIntegrationEventHandler.Called.WaitOne(Debugger.IsAttached ? int.MaxValue : 3000));
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