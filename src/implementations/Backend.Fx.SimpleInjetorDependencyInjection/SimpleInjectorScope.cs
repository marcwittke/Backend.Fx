using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using SimpleInjector;

namespace Backend.Fx.SimpleInjectorDependencyInjection
{
    public sealed class SimpleInjectorScope : IScope
    {
        private static readonly ILogger Logger = LogManager.Create<SimpleInjectorScope>();
        private readonly Scope _scope;

        public SimpleInjectorScope(Scope scope, IIdentity identity, TenantId tenantId)
        {
            LogManager.BeginActivity();
            _scope = scope;
            Logger.Info($"Began new scope for identity [{identity.Name}] and tenant[{(tenantId.HasValue ? tenantId.Value.ToString() : "")}]");
        }
        
        public TService GetInstance<TService>() where TService : class
        {
            return _scope.Container.GetInstance<TService>();
        }

        public object GetInstance(Type serviceType)
        {
            return _scope.Container.GetInstance(serviceType);
        }

        public IEnumerable<TService> GetAllInstances<TService>() where TService : class
        {
            return _scope.Container.GetAllInstances<TService>();
        }

        public IEnumerable GetAllInstances(Type serviceType)
        {
            return _scope.Container.GetAllInstances(serviceType);
        }

        public void Dispose()
        {
            _scope?.Dispose();
        }
    }
}