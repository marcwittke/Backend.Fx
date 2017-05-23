namespace Backend.Fx.Patterns.DependencyInjection
{
    using System;
    using Environment.MultiTenancy;
    using EventAggregation;
    using Jobs;

    public interface IRuntime : IEventHandlerProvider, IJobExecutor, ITenantInitializer, IScopeManager, IDisposable
    {
        /// <summary>
        /// Only allowed for framework integration (or tests). NEVER (!) access the container from the domain or 
        /// application logic, this would result in the Service Locator anti pattern, described here:
        /// http://blog.ploeh.dk/2010/02/03/ServiceLocatorisanAnti-Pattern/
        /// </summary>
        object GetInstance(Type serviceType);

        /// <summary>
        /// Only allowed for framework integration (or tests). NEVER (!) access the container from the domain or 
        /// application logic, this would result in the Service Locator anti pattern, described here:
        /// http://blog.ploeh.dk/2010/02/03/ServiceLocatorisanAnti-Pattern/
        /// </summary>
        T GetInstance<T>() where T : class;
    }
}
