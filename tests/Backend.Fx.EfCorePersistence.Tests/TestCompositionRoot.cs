using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;

namespace Backend.Fx.EfCorePersistence.Tests
{
    public class TestCompositionRoot : ICompositionRoot
    {
        private readonly Dictionary<Type, object[]> _registry;

        public TestCompositionRoot(params object[] instances)
        {
            _registry = instances
                .GroupBy(i => i.GetType())
                .ToDictionary(grp => grp.Key, grp => grp.ToArray());
        }

        public void Dispose()
        { }

        public IEnumerable<IDomainEventHandler<TDomainEvent>> GetAllEventHandlers<TDomainEvent>()
            where TDomainEvent : IDomainEvent
        {
            return new IDomainEventHandler<TDomainEvent>[0];
        }

        public object GetInstance(Type serviceType)
        {
            if (_registry.TryGetValue(serviceType, out var instances))
            {
                return instances.FirstOrDefault();
            }

            return null;
        }

        public T GetInstance<T>() where T : class
        {
            return (T)GetInstance(typeof(T));
        }

        public IEnumerable<T> GetInstances<T>() where T : class
        {
            if (_registry.TryGetValue(typeof(T), out var instances))
            {
                return instances.Cast<T>();
            }

            return new T[0];
        }

        public void Verify()
        { }

        public void RegisterModules(params IModule[] modules)
        { }
    }
}