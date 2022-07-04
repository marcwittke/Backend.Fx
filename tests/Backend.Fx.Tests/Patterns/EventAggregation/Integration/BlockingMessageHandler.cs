using System.Diagnostics;
using System.Threading;
using Backend.Fx.Features.MessageBus;
using Xunit;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    public class BlockingMessageHandler : IIntegrationMessageHandler<TestIntegrationEvent>
    {
        private readonly ManualResetEvent _manualResetEvent;

        public BlockingMessageHandler(ManualResetEvent manualResetEvent)
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