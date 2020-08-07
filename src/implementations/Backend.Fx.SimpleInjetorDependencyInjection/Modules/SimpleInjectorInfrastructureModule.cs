using System;
using System.Security.Principal;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;
using SimpleInjector;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Modules
{
    public class SimpleInjectorInfrastructureModule : IInfrastructureModule
    {
        private readonly Container _container;
        
        public SimpleInjectorInfrastructureModule(Container container)
        {
            _container = container;
        }
        public void RegisterCorrelationHolder<T>() where T : class, ICurrentTHolder<Correlation>
        {
            _container.Register<T>();
        }

        public void RegisterDomainEventAggregator(Func<IDomainEventAggregator> factory)
        {
            _container.Register(factory);
        }

        public void RegisterIdentityHolder<T>() where T : class, ICurrentTHolder<IIdentity>
        {
            _container.Register<T>();
        }

        public void RegisterMessageBusScope(Func<IMessageBusScope> factory)
        {
            _container.Register(factory);
        }

        public void RegisterTenantHolder<T>() where T : class, ICurrentTHolder<TenantId>
        {
            _container.Register<T>();
        }
    }
}