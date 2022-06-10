using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Patterns.DependencyInjection
{
    /// <summary>
    /// Encapsulates the injection framework of choice. The implementation follows the Register/Resolve/Release pattern.
    /// Usage of this interface is only allowed for framework integration (or tests). NEVER (!) access the injector from
    /// the domain or application logic, this would result in the Service Locator anti pattern, described here:
    /// http://blog.ploeh.dk/2010/02/03/ServiceLocatorisanAnti-Pattern/
    /// </summary>
    public interface ICompositionRoot : IDisposable
    {
        void Verify();

        void RegisterModules(params IModule[] modules);
        
        void RegisterServiceDescriptor(ServiceDescriptor serviceDescriptor);
        
        void RegisterServiceDescriptors(IEnumerable<ServiceDescriptor> serviceDescriptors);

        IServiceScope BeginScope();
        
        /// <summary>
        /// Access to the container's resolution functionality
        /// </summary>
        IServiceProvider ServiceProvider { get; }
    }
}