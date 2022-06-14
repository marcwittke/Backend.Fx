using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.MicrosoftDependencyInjection
{
    public class MicrosoftCompositionRoot : CompositionRoot
    {
        private static readonly ILogger Logger = Log.Create<MicrosoftCompositionRoot>();
        private readonly IServiceCollection _serviceCollection = new ServiceCollection();
        private readonly Lazy<IServiceProvider> _serviceProvider;

        public MicrosoftCompositionRoot()
        {
            _serviceProvider = new Lazy<IServiceProvider>(() =>
            {
                Logger.LogInformation("Building Microsoft ServiceProvider");
                return _serviceCollection.BuildServiceProvider();
            });
        }

        public override IServiceProvider ServiceProvider => _serviceProvider.Value;

        public override void Verify()
        {
            // ensure creation of lazy service provider
            var unused = _serviceProvider.Value;
        }

        public override void Register(ServiceDescriptor serviceDescriptor)
        {
            if (_serviceProvider.IsValueCreated)
            {
                throw new InvalidOperationException("Service provider has been built and cannot be changed any more.");
            }

            var existingRegistration = _serviceCollection
                .SingleOrDefault(sd => sd.ServiceType == serviceDescriptor.ServiceType);
            
            if (existingRegistration == null)
            {
                serviceDescriptor.LogDetails(Logger, "Adding");
                _serviceCollection.Add(serviceDescriptor);
            }
            else
            {
                serviceDescriptor.LogDetails(Logger, "Replacing");
                _serviceCollection.Replace(serviceDescriptor);
            }
        }

        public override void RegisterDecorator(ServiceDescriptor serviceDescriptor)
        {
            serviceDescriptor.LogDetails(Logger, "Adding decorator");
            _serviceCollection.Decorate(serviceDescriptor.ServiceType, serviceDescriptor.ImplementationType);
        }

        public override void RegisterCollection(IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            foreach (var serviceDescriptor in serviceDescriptors)
            {
                serviceDescriptor.LogDetails(Logger, "Adding");
                _serviceCollection.Add(serviceDescriptor);
            }
        }

        public override IServiceScope BeginScope()
        {
            return ServiceProvider.CreateScope();
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}