using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
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
        private readonly Lazy<Container> _container;
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
            Logger.LogInformation("Initializing SimpleInjector");
            ScopedLifestyle = scopedLifestyle;
            _container = new Lazy<Container>(() =>
            {
                Logger.LogInformation("Building SimpleInjector Container");
                var container = new Container();
                container.Options.LifestyleSelectionBehavior = lifestyleBehavior;
                container.Options.DefaultScopedLifestyle = ScopedLifestyle;

                foreach (var serviceDescriptor in _services)
                {
                    serviceDescriptor.LogDetails(Logger, "Adding");

                    if (serviceDescriptor.ImplementationType != null)
                    {
                        container.Register(
                            serviceDescriptor.ServiceType,
                            serviceDescriptor.ImplementationType,
                            serviceDescriptor.Lifetime.Translate());
                    }
                    else if (serviceDescriptor.ImplementationFactory != null)
                    {
                        container.Register(
                            serviceDescriptor.ServiceType,
                            () => serviceDescriptor.ImplementationFactory(container),
                            serviceDescriptor.Lifetime.Translate());
                    }
                    else if (serviceDescriptor.ImplementationInstance != null &&
                             serviceDescriptor.Lifetime == ServiceLifetime.Singleton)
                    {
                        container.RegisterInstance(serviceDescriptor.ServiceType,
                            serviceDescriptor.ImplementationInstance);
                    }
                    else
                    {
                        throw new InvalidOperationException("Bad service descriptor");
                    }
                }

                foreach (var serviceDescriptor in _decorators)
                {
                    serviceDescriptor.LogDetails(Logger, "Adding decorator");

                    container.RegisterDecorator(
                        serviceDescriptor.ServiceType,
                        serviceDescriptor.ImplementationType,
                        serviceDescriptor.Lifetime.Translate());
                }

                foreach (var serviceDescriptors in _serviceCollections)
                {
                    Logger.Debug("Adding {Lifetime} collection registration: {ServiceType}: {ImplementationType}",
                        serviceDescriptors[0].Lifetime.ToString(),
                        serviceDescriptors[0].ServiceType.Name,
                        $"[{string.Join(",", serviceDescriptors.Select(sd => sd.GetImplementationTypeDescription()))}]");

                    foreach (var serviceDescriptor in serviceDescriptors)
                    {
                        container.Collection.Append(
                            serviceDescriptor.ServiceType,
                            serviceDescriptor.ImplementationType,
                            serviceDescriptor.Lifetime.Translate());
                    }
                }

                // needed to support extension method IServiceProvider.CreateScope() 
                container.RegisterInstance<IServiceScopeFactory>(new SimpleInjectorServiceScopeFactory(container));

                return container;
            });
        }

        public ScopedLifestyle ScopedLifestyle { get; }

        public Container Container => _container.Value;

        #region ICompositionRoot implementation

        public override void Register(ServiceDescriptor serviceDescriptor)
        {
            if (_container.IsValueCreated)
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
            if (_container.IsValueCreated)
            {
                throw new InvalidOperationException("Container has been built and cannot be changed any more.");
            }

            _decorators.Add(serviceDescriptor);
        }

        public override void RegisterCollection(IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            if (_container.IsValueCreated)
            {
                throw new InvalidOperationException("Container has been built and cannot be changed any more.");
            }

            var serviceDescriptorArray = serviceDescriptors as ServiceDescriptor[] ?? serviceDescriptors.ToArray();

            if (serviceDescriptorArray.Length == 0)
            {
                Logger.Warn("Skipping registration of empty collection");
                return;
            }

            if (serviceDescriptorArray.Select(sd => sd.ServiceType).Distinct().Count() > 1)
            {
                throw new InvalidOperationException(
                    "To register a collection of services they must implement the same service type");
            }

            _serviceCollections.Add(serviceDescriptorArray);
        }

        public override void Verify()
        {
            Logger.LogInformation("container is being verified");
            try
            {
                _container.Value.Verify(VerificationOption.VerifyAndDiagnose);
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
            return _container.Value.CreateScope();
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

        #endregion

        #region IDisposable implementation

        protected override void Dispose(bool disposing)
        {
            if (disposing && _container.IsValueCreated)
            {
                _container.Value.Dispose();
            }
        }

        #endregion
    }
}