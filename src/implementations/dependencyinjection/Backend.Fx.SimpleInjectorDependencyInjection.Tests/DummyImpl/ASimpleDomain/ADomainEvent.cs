using System;
using System.Collections.Generic;
using Backend.Fx.Patterns.EventAggregation.Domain;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.ASimpleDomain
{
    public class ADomainEvent : IDomainEvent
    {
        public HashSet<Type> HandledBy { get; } = new();
    }
}
