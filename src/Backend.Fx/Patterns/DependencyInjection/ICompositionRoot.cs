namespace Backend.Fx.Patterns.DependencyInjection
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Encapsulates the injection framework of choice. The implementation follows the Register/Resolve/Release pattern.
    /// Usage of this interface is only allowed for framework integration (or tests). NEVER (!) access the injector from
    /// the domain or application logic, this would result in the Service Locator anti pattern, described here:
    /// http://blog.ploeh.dk/2010/02/03/ServiceLocatorisanAnti-Pattern/
    /// </summary>
    public interface ICompositionRoot : IDisposable
    {
        /// <summary>
        /// Gets a service instance by providing its type
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        object GetInstance(Type serviceType);

        /// <summary>
        /// Gets a service instance by providing its type via generic type parameter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetInstance<T>() where T : class;

        IEnumerable<T> GetInstances<T>() where T : class;

        void Verify();

        void RegisterModules(params IModule[] modules);
    }
}
