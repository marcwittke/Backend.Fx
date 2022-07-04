﻿using System.Threading;
using Backend.Fx.Features.MessageBus;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    public class LongRunningMessageHandler : IIntegrationMessageHandler<TestIntegrationEvent>
    {
        private readonly IIntegrationMessageHandler<TestIntegrationEvent> _handler;

        public LongRunningMessageHandler(IIntegrationMessageHandler<TestIntegrationEvent> handler)
        {
            _handler = handler;
        }

        public void Handle(TestIntegrationEvent eventData)
        {
            Thread.Sleep(1000);
            _handler.Handle(eventData);
        }
    }
}