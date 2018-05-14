namespace Backend.Fx.Bootstrapping
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Security.Principal;
    using Environment.MultiTenancy;
    using Logging;
    using Patterns.DependencyInjection;
    using Patterns.UnitOfWork;
    using SimpleInjector;

    public sealed class SimpleInjectorScope : IScope
    {
        private static readonly ILogger Logger = LogManager.Create<SimpleInjectorScope>();
        private readonly Scope scope;

        public SimpleInjectorScope(Scope scope, IIdentity identity, TenantId tenantId)
        {
            LogManager.BeginActivity();
            this.scope = scope;
            Logger.Info($"Began new scope for identity [{identity.Name}] and tenant[{(tenantId.HasValue ? tenantId.Value.ToString() : "")}]");
        }

        public IUnitOfWork BeginUnitOfWork(bool beginAsReadonlyUnitOfWork)
        {
            IUnitOfWork unitOfWork = beginAsReadonlyUnitOfWork
                             ? scope.Container.GetInstance<IReadonlyUnitOfWork>()
                             : scope.Container.GetInstance<IUnitOfWork>();

            unitOfWork.Begin();
            return unitOfWork;
        }

        public TService GetInstance<TService>() where TService : class
        {
            return scope.Container.GetInstance<TService>();
        }

        public object GetInstance(Type serviceType)
        {
            return scope.Container.GetInstance(serviceType);
        }

        public IEnumerable<TService> GetAllInstances<TService>() where TService : class
        {
            return scope.Container.GetAllInstances<TService>();
        }

        public IEnumerable GetAllInstance(Type serviceType)
        {
            return scope.Container.GetAllInstances(serviceType);
        }

        public void Dispose()
        {
            scope?.Dispose();
        }
    }
}