using System;
using System.Security.Principal;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.Patterns.DependencyInjection
{
    public interface IInfrastructureModule
    {
         void RegisterCorrelationHolder<T>() where T : class, ICurrentTHolder<Correlation>;

         void RegisterDomainEventAggregator(Func<IDomainEventAggregator> factory);

         void RegisterIdentityHolder<T>() where T : class, ICurrentTHolder<IIdentity>;

         void RegisterMessageBusScope(Func<IMessageBusScope> factory);

         void RegisterTenantHolder<T>() where T : class, ICurrentTHolder<TenantId>;
    }
}