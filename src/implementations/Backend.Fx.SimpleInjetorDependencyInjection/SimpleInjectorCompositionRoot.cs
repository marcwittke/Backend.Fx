using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.SimpleInjectorDependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleInjector;
using SimpleInjector.Advanced;
using SimpleInjector.Lifestyles;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.SimpleInjectorDependencyInjection
{
    /// <summary>
    ///     Provides a reusable composition root assuming Simple Injector as container
    /// </summary>
    public class SimpleInjectorCompositionRoot : ICompositionRoot
    {
        private static readonly ILogger Logger = Log.Create<SimpleInjectorCompositionRoot>();

        /// <summary>
        /// This constructor creates a composition root that prefers scoped lifestyle
        /// </summary>
        public SimpleInjectorCompositionRoot() 
            : this(new ScopedLifestyleBehavior(), new AsyncScopedLifestyle())
        {}

        public SimpleInjectorCompositionRoot(ILifestyleSelectionBehavior lifestyleBehavior, ScopedLifestyle scopedLifestyle)
        {
            Logger.LogInformation("Initializing SimpleInjector");
            ScopedLifestyle = scopedLifestyle;
            Container.Options.LifestyleSelectionBehavior = lifestyleBehavior;
            Container.Options.DefaultScopedLifestyle = ScopedLifestyle;
            ServiceProvider = Container;
        }

        public Container Container { get; } = new Container();

        internal ScopedLifestyle ScopedLifestyle { get; }

        #region ICompositionRoot implementation

        public void RegisterModules(params IModule[] modules)
        {
            foreach (var module in modules)
            {
                Logger.LogInformation("Registering {Module}", module.GetType().Name);
                module.Register(this);
            }
        }

        public void RegisterServiceDescriptor(ServiceDescriptor serviceDescriptor)
        {
            if (serviceDescriptor.ImplementationType != null)
            {
                Container.Register(
                    serviceDescriptor.ServiceType, 
                    serviceDescriptor.ImplementationType,
                    serviceDescriptor.Lifetime.Translate());
            }
            else if (serviceDescriptor.ImplementationFactory != null)
            {
                Container.Register(
                    serviceDescriptor.ServiceType,
                    () => serviceDescriptor.ImplementationFactory(Container),
                    serviceDescriptor.Lifetime.Translate());
            }
            else if (serviceDescriptor.ImplementationInstance != null &&
                     serviceDescriptor.Lifetime == ServiceLifetime.Singleton)
            {
                Container.RegisterInstance(serviceDescriptor.ServiceType, serviceDescriptor.ImplementationInstance);
            }
            else
            {
                throw new InvalidOperationException("Bad service descriptor");
            }
        }

        public void RegisterServiceDescriptors(IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            var serviceDescriptorArray = serviceDescriptors as ServiceDescriptor[] ?? serviceDescriptors.ToArray();
            
            if (serviceDescriptorArray.Select(sd => sd.ServiceType).Distinct().Count() > 1)
            {
                throw new InvalidOperationException("To register a collection of services they must implement the same service type");
            }

            foreach (var serviceDescriptor in serviceDescriptorArray)
            {
                Container.Collection.Append(
                    serviceDescriptor.ServiceType,
                    serviceDescriptor.ImplementationType,
                    serviceDescriptor.Lifetime.Translate());
            }
        }

        public void Verify()
        {
            Logger.LogInformation("container is being verified");
            try
            {
                Container.Verify(VerificationOption.VerifyAndDiagnose);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "container configuration invalid");
                throw;
            }
        }

        /// <inheritdoc />
        public IServiceScope BeginScope()
        {
            return new SimpleInjectorServiceScope(AsyncScopedLifestyle.BeginScope(Container));
        }

        public IServiceProvider ServiceProvider { get; }

        public Scope GetCurrentScope()
        {
            return ScopedLifestyle.GetCurrentScope(Container);
        }
        #endregion

        #region IDisposable implementation
        public void Dispose()
        {
            Container?.Dispose();
        }
        #endregion

        /// <summary>
        ///     A behavior that defaults to scoped life style for injected instances
        /// </summary>
        private class ScopedLifestyleBehavior : ILifestyleSelectionBehavior
        {
            public Lifestyle SelectLifestyle(Type implementationType)
            {
                return Lifestyle.Scoped;
            }
        }
    }
}