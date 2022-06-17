using System;
using JetBrains.Annotations;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    [PublicAPI]
    public interface IMessageNameProvider
    {
        [NotNull]
        string GetMessageName<T>();
        [NotNull]
        string GetMessageName(Type t);
        [NotNull]
        string GetMessageName(IIntegrationEvent integrationEvent);
    }
}