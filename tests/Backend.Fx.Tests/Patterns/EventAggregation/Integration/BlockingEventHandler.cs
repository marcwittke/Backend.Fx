using System.Diagnostics;
using System.Threading;
using Backend.Fx.Extensions.MessageBus;
using Xunit;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    public class BlockingEventHandler : IIntegrationEventHandler<TestIntegrationEvent>
    {
        private readonly ManualResetEvent _manualResetEvent;

        public BlockingEventHandler(ManualResetEvent manualResetEvent)
        {
            _manualResetEvent = manualResetEvent;
        }

        public void Handle(TestIntegrationEvent eventData)
        {
            Assert.True(
                _manualResetEvent.WaitOne(Debugger.IsAttached ? int.MaxValue : 1000), 
                "The BlockingMessageHandler was not reset.");
        }
    }
}