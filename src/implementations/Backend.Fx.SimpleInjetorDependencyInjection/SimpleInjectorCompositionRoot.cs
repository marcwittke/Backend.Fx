using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Fx.DependencyInjection;
using Backend.Fx.Logging;
using JetBrains.Annotations;
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
    [PublicAPI]
    public class SimpleInjectorCompositionRoot : CompositionRoot
    {
        private readonly ILogger _logger = Log.Create<SimpleInjectorCompositionRoot>();
        private readonly IList<ServiceDescriptor> _services = new List<ServiceDescriptor>();
        private readonly IList<ServiceDescriptor> _decorators = new List<ServiceDescriptor>();
        private readonly IList<ServiceDescriptor[]> _serviceCollections = new List<ServiceDescriptor[]>();

        /// <summary>
        /// This constructor creates a composition root that prefers scoped lifestyle
        /// </summary>
        public SimpleInjectorCompositionRoot()
            : this(new ScopedLifestyleBehavior(), new AsyncScopedLifestyle())
        {
        }

        public SimpleInjectorCompositionRoot(
            ILifestyleSelectionBehavior lifestyleBehavior,
            ScopedLifestyle scopedLifestyle)
        {
            _logger.LogInformation("Initializing SimpleInjector");
            ScopedLifestyle = scopedLifestyle;

            Container.Options.LifestyleSelectionBehavior = lifestyleBehavior;
            Container.Options.DefaultScopedLifestyle = scopedLifestyle;

            // required to support extension method IServiceProvider.CreateScope() 
            Container.RegisterInstance<IServiceScopeFactory>(new SimpleInjectorServiceScopeFactory(Container));
        }

        public ScopedLifestyle ScopedLifestyle { get; }

        public Container Container { get; } = new Container();

        #region ICompositionRoot implementation

        public override void Register(ServiceDescriptor serviceDescriptor)
        {
            if (Container.IsLocked)
            {
                throw new InvalidOperationException("Container has been built and cannot be changed any more.");
            }

            foreach (var descriptor in _services.Where(sd => sd.ServiceType == serviceDescriptor.ServiceType).ToArray())
            {
                _services.Remove(descriptor);
            }

            _services.Add(serviceDescriptor);
        }

        public override void RegisterDecorator(ServiceDescriptor serviceDescriptor)
        {
            if (Container.IsLocked)
            {
                throw new InvalidOperationException("Container has been built and cannot be changed any more.");
            }

            _decorators.Add(serviceDescriptor);
        }

        public override void RegisterCollection(IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            if (Container.IsLocked)
            {
                throw new InvalidOperationException("Container has been built and cannot be changed any more.");
            }

            var serviceDescriptorArray = serviceDescriptors as ServiceDescriptor[] ?? serviceDescriptors.ToArray();

            if (serviceDescriptorArray.Select(sd => sd.ServiceType).Distinct().Count() > 1)
            {
                throw new InvalidOperationException(
                    "To register a collection of services they must implement the same service type");
            }

            _serviceCollections.Add(serviceDescriptorArray);
        }

        public override void Verify()
        {
            FillContainer();

            _logger.LogInformation("Verifying container");
            Container.Verify(VerificationOption.VerifyAndDiagnose);
        }

        /// <inheritdoc />
        public override IServiceScope BeginScope()
        {
            return Container.CreateScope();
        }

        public override IServiceProvider ServiceProvider => Container;

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

        private void FillContainer()
        {
            _logger.LogInformation("Registering services with container");
            foreach (var serviceDescriptor in _services)
            {
                if (serviceDescriptor.ImplementationType != null)
                {
                    Container.Register(
                        serviceDescriptor.ServiceType,
                        serviceDescriptor.ImplementationType,
                        serviceDescriptor.Lifetime.MapLifestyle());
                }
                else if (serviceDescriptor.ImplementationFactory != null)
                {
                    Container.Register(
                        serviceDescriptor.ServiceType,
                        () => serviceDescriptor.ImplementationFactory(Container),
                        serviceDescriptor.Lifetime.MapLifestyle());
                }
                else if (serviceDescriptor.ImplementationInstance != null &&
                         serviceDescriptor.Lifetime == ServiceLifetime.Singleton)
                {
                    Container.RegisterInstance(serviceDescriptor.ServiceType,
                        serviceDescriptor.ImplementationInstance);
                }
                else
                {
                    throw new InvalidOperationException("Bad service descriptor");
                }
            }

            foreach (var serviceDescriptor in _decorators)
            {
                if (serviceDescriptor.ImplementationType != null)
                {
                    Container.RegisterDecorator(
                        serviceDescriptor.ServiceType,
                        serviceDescriptor.ImplementationType,
                        serviceDescriptor.Lifetime.MapLifestyle());
                }
                else
                {
                    throw new InvalidOperationException("Can only register decorators by type");
                }
            }

            foreach (var serviceDescriptors in _serviceCollections)
            {
                foreach (var serviceDescriptor in serviceDescriptors)
                {
                    Container.Collection.Append(
                        serviceDescriptor.ServiceType,
                        serviceDescriptor.ImplementationType,
                        serviceDescriptor.Lifetime.MapLifestyle());
                }
            }
        }

        #endregion

        #region IDisposable implementation

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Container.Dispose();
            }
        }

        #endregion
    }
}