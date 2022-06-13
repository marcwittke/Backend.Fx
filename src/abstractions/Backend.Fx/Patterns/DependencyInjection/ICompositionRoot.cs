using System;
using System.Collections.Generic;
using Backend.Fx.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

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

    
    public abstract class CompositionRoot : ICompositionRoot
    {
        private static readonly ILogger Logger = Log.Create<CompositionRoot>();
        
        public abstract IServiceProvider ServiceProvider { get; }
        
        public abstract void Verify();

        public virtual void RegisterModules(params IModule[] modules)
        {
            foreach (var module in modules)
            {
                Logger.LogInformation("Registering {Module}", module.GetType().Name);
                module.Register(this);
            }
        }

        public abstract void RegisterServiceDescriptor(ServiceDescriptor serviceDescriptor);
        
        public abstract void RegisterServiceDescriptors(IEnumerable<ServiceDescriptor> serviceDescriptors);
        
        public abstract IServiceScope BeginScope();

        protected abstract void Dispose(bool disposing);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}