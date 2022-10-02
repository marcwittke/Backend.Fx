using System;
using System.Collections.Generic;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.DependencyInjection
{
    /// <summary>
    /// Encapsulates the injection framework of choice. The implementation follows the Register/Resolve/Release pattern.
    /// Usage of this interface is only allowed for framework integration (or tests). NEVER (!) access the injector from
    /// the domain or application logic, this would result in the Service Locator anti pattern, described here:
    /// http://blog.ploeh.dk/2010/02/03/ServiceLocatorisanAnti-Pattern/
    /// </summary>
    [PublicAPI]
    public interface ICompositionRoot : IDisposable
    {
        void Verify();

        void RegisterModules(params IModule[] modules);

        void Register(ServiceDescriptor serviceDescriptor);

        void RegisterDecorator(ServiceDescriptor serviceDescriptor);

        void RegisterCollection(IEnumerable<ServiceDescriptor> serviceDescriptors);

        IServiceScope BeginScope();

        /// <summary>
        /// Access to the container's resolution functionality
        /// </summary>
        IServiceProvider ServiceProvider { get; }
    }

    [PublicAPI]
    public abstract class CompositionRoot : ICompositionRoot
    {
        private static readonly ILogger Logger = Log.Create<CompositionRoot>();
        
        public abstract IServiceProvider ServiceProvider { get; }
        
        public abstract void Verify();

        public virtual void RegisterModules(params IModule[] modules)
        {
            foreach (var module in modules)
            {
                Logger.LogInformation("Registering {@Module}", module);
                module.Register(this);
            }
        }

        public abstract void Register(ServiceDescriptor serviceDescriptor);
        
        public abstract void RegisterDecorator(ServiceDescriptor serviceDescriptor);

        public abstract void RegisterCollection(IEnumerable<ServiceDescriptor> serviceDescriptors);
        
        public abstract IServiceScope BeginScope();

        protected abstract void Dispose(bool disposing);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}