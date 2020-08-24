using System;

namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    public interface IMessageNameProvider
    {
        string GetMessageName<T>();
        string GetMessageName(Type t);
        string GetMessageName(IIntegrationEvent integrationEvent);
    }
}