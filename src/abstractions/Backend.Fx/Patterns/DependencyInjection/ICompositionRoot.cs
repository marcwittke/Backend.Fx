namespace Backend.Fx.Patterns.DependencyInjection
{
    using System;
    using EventAggregation.Domain;

    /// <summary>
    /// Encapsulates the injection framework of choice. The implementation follows the Register/Resolve/Release pattern.
    /// Usage of this interface is only allowed for framework integration (or tests). NEVER (!) access the injector from
    /// the domain or application logic, this would result in the Service Locator anti pattern, described here:
    /// http://blog.ploeh.dk/2010/02/03/ServiceLocatorisanAnti-Pattern/
    /// </summary>
    public interface ICompositionRoot : IDisposable, IDomainEventHandlerProvider
    {
        void Verify();

        void RegisterModules(params IModule[] modules);

        IInjectionScope BeginScope();
        IInstanceProvider InstanceProvider { get; }

        /// <summary>
        /// Gets the current correlation, when inside a scope, otherwise this method will return false
        /// </summary>
        /// <param name="correlation"></param>
        /// <returns></returns>
        bool TryGetCurrentCorrelation(out Correlation correlation);
    }
}
