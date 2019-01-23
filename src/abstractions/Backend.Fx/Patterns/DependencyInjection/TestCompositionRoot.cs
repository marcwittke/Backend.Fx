using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.EventAggregation.Domain;

namespace Backend.Fx.Patterns.DependencyInjection
{
    public class TestCompositionRoot : ICompositionRoot, IScopeManager, IScope
    {
        private readonly Dictionary<Type, Func<object>> _registrations = new Dictionary<Type, Func<object>>();
        private readonly Dictionary<Type, Func<object>[]> _collectionRegistrations = new Dictionary<Type, Func<object>[]>();

        public object GetInstance(Type serviceType)
        {
            if (_registrations.TryGetValue(serviceType, out var creator))
            {
                return creator();
            }

            if (!serviceType.GetTypeInfo().IsAbstract)
            {
                return CreateInstance(serviceType);
            }

            throw new InvalidOperationException("No registration for " + serviceType);
        }

        public IEnumerable<TService> GetAllInstances<TService>() where TService : class
        {
            return GetInstances<TService>();
        }

        public IEnumerable GetAllInstances(Type serviceType)
        {
            return GetInstances(serviceType);
        }

        public T GetInstance<T>() where T : class
        {
            return (T)GetInstance(typeof(T));
        }

        public IEnumerable<T> GetInstances<T>() where T : class
        {
            return GetInstances(typeof(T)).Cast<T>();
        }

        public IEnumerable GetInstances(Type serviceType) 
        {
            if (_collectionRegistrations.TryGetValue(serviceType, out var creator))
            {
                return creator.Select(c => c());
            }

            throw new InvalidOperationException("No collection registration for " + serviceType);
        }

        public void Verify()
        {
        }

        public void RegisterModules(params IModule[] modules)
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        { }

        public IEnumerable<IDomainEventHandler<TDomainEvent>> GetAllEventHandlers<TDomainEvent>()
            where TDomainEvent : IDomainEvent
        {
            return GetInstances<IDomainEventHandler<TDomainEvent>>();
        }

        public void Register<TService, TImpl>() where TImpl : TService
        {
            _registrations.Add(typeof(TService), () => GetInstance(typeof(TImpl)));
        }

        public void Register<TService>(Func<TService> instanceCreator)
        {
            _registrations.Add(typeof(TService), () => instanceCreator());
        }

        public void RegisterSingleton<TService>(TService instance)
        {
            _registrations.Add(typeof(TService), () => instance);
        }

        public void RegisterSingleton<TService>(Func<TService> instanceCreator)
        {
            var lazy = new Lazy<TService>(instanceCreator);
            Register(() => lazy.Value);
        }

        public void RegisterCollection<TService>(Assembly assembly)
        {
            var funcs = assembly
                .GetExportedTypes()
                .Where(ti => ti.GetTypeInfo().IsClass && !ti.GetTypeInfo().IsAbstract && typeof(TService).IsAssignableFrom(ti))
                .Select<Type, Func<object>>(ti => () => GetInstance(ti))
                .ToArray();

            _collectionRegistrations.Add(typeof(TService), funcs);
        }
        
        private object CreateInstance(Type implementationType)
        {
            var ctor = implementationType.GetConstructors().Single();
            var parameterTypes = ctor.GetParameters().Select(p => p.ParameterType);
            var dependencies = parameterTypes.Select(GetInstance).ToArray();
            return Activator.CreateInstance(implementationType, dependencies);
        }

        public IScope BeginScope(IIdentity identity, TenantId tenantId)
        {
            return this;
        }

        public IScope GetCurrentScope()
        {
            return this;
        }
    }
}