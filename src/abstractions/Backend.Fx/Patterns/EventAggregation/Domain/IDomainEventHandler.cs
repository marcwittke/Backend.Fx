﻿using System.Threading.Tasks;

namespace Backend.Fx.Patterns.EventAggregation.Domain
{
    public interface IDomainEventHandler<in TDomainEvent> where TDomainEvent : IDomainEvent
    {
        Task HandleAsync(TDomainEvent domainEvent);
    }
}