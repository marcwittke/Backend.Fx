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
    public class SimpleInjectorCompositionRoot : CompositionRoot
    {
        private static readonly ILogger Logger = Log.Create<SimpleInjectorCompositionRoot>();
        private readonly Container _container = new Container();
        private readonly ScopedLifestyle _scopedLifestyle;

        /// <summary>
        /// This constructor creates a composition root that prefers scoped lifestyle
        /// </summary>
        public SimpleInjectorCompositionRoot() 
            : this(new ScopedLifestyleBehavior(), new AsyncScopedLifestyle())
        {}

        public SimpleInjectorCompositionRoot(ILifestyleSelectionBehavior lifestyleBehavior, ScopedLifestyle scopedLifestyle)
        {
            Logger.LogInformation("Initializing SimpleInjector");
            _scopedLifestyle = scopedLifestyle;
            _container.Options.LifestyleSelectionBehavior = lifestyleBehavior;
            _container.Options.DefaultScopedLifestyle = _scopedLifestyle;
        }

        
        #region ICompositionRoot implementation

        public override void RegisterServiceDescriptor(ServiceDescriptor serviceDescriptor)
        {
            if (serviceDescriptor.ImplementationType != null)
            {
                _container.Register(
                    serviceDescriptor.ServiceType, 
                    serviceDescriptor.ImplementationType,
                    serviceDescriptor.Lifetime.Translate());
            }
            else if (serviceDescriptor.ImplementationFactory != null)
            {
                _container.Register(
                    serviceDescriptor.ServiceType,
                    () => serviceDescriptor.ImplementationFactory(_container),
                    serviceDescriptor.Lifetime.Translate());
            }
            else if (serviceDescriptor.ImplementationInstance != null &&
                     serviceDescriptor.Lifetime == ServiceLifetime.Singleton)
            {
                _container.RegisterInstance(serviceDescriptor.ServiceType, serviceDescriptor.ImplementationInstance);
            }
            else
            {
                throw new InvalidOperationException("Bad service descriptor");
            }
        }

        public override void RegisterServiceDescriptors(IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            var serviceDescriptorArray = serviceDescriptors as ServiceDescriptor[] ?? serviceDescriptors.ToArray();
            
            if (serviceDescriptorArray.Select(sd => sd.ServiceType).Distinct().Count() > 1)
            {
                throw new InvalidOperationException("To register a collection of services they must implement the same service type");
            }

            foreach (var serviceDescriptor in serviceDescriptorArray)
            {
                _container.Collection.Append(
                    serviceDescriptor.ServiceType,
                    serviceDescriptor.ImplementationType,
                    serviceDescriptor.Lifetime.Translate());
            }
        }

        public override void Verify()
        {
            Logger.LogInformation("container is being verified");
            try
            {
                _container.Verify(VerificationOption.VerifyAndDiagnose);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "container configuration invalid");
                throw;
            }
        }

        /// <inheritdoc />
        public override IServiceScope BeginScope()
        {
            return new SimpleInjectorServiceScope(AsyncScopedLifestyle.BeginScope(_container));
        }

        public override IServiceProvider ServiceProvider => _container;

        public Scope GetCurrentScope()
        {
            return _scopedLifestyle.GetCurrentScope(_container);
        }
        
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
        #endregion

        #region IDisposable implementation
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _container?.Dispose();
            }
        }
        #endregion
    }
}