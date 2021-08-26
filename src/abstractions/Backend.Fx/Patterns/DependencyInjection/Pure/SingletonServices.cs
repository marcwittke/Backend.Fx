using System;
using System.Reflection;
using System.Security.Principal;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.IdGeneration;

namespace Backend.Fx.Patterns.DependencyInjection.Pure
{
    /// <summary>
    /// A strongly typed registry of singleton instances, without the need for a dependency injection container.
    /// </summary>
    /// <remarks>This pattern is called "Pure DI", see https://blog.ploeh.dk/2012/11/06/WhentouseaDIContainer/ for details.
    /// It is very useful in testing scenarios.</remarks>
    public interface ISingletonServices
    {
        AdjustableClock Clock { get; }
        
        IEntityIdGenerator EntityIdGenerator { get; }

        Assembly[] Assemblies { get; }
    }

    public interface ISingletonServices<out TScopedServices> : ISingletonServices where TScopedServices : IScopedServices
    {
        TScopedServices BeginScope(TenantId tenantId, IIdentity identity = null);
    }

    public abstract class SingletonServices<TScopedServices> : ISingletonServices<TScopedServices>
        where TScopedServices : IScopedServices
    {
        protected SingletonServices(params Assembly[] assemblies)
        {
            Assemblies = assemblies ?? Array.Empty<Assembly>();
        }

        public Assembly[] Assemblies { get; }
        
        public virtual AdjustableClock Clock { get; } = new AdjustableClock(new WallClock());

        public abstract IEntityIdGenerator EntityIdGenerator { get; }

        public abstract TScopedServices BeginScope(TenantId tenantId, IIdentity identity = null);
    }
}