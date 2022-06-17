﻿using JetBrains.Annotations;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    [PublicAPI]
    public interface IIntegrationMessageHandler
    {
        void Handle(dynamic eventData);
    }

    [PublicAPI]
    public interface IIntegrationMessageHandler<in TEvent> where TEvent : IIntegrationEvent
    {
        void Handle(TEvent eventData);
    }
}