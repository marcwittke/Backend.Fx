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
        private readonly Lazy<IServiceProvider> _serviceProvider;

        public MicrosoftCompositionRoot()
        {
            _serviceProvider = new Lazy<IServiceProvider>(() =>
            {
                Logger.LogInformation("Building Microsoft ServiceProvider");
                return ServiceCollection.BuildServiceProvider(
                    new ServiceProviderOptions
                    {
                        ValidateScopes = true,
                        ValidateOnBuild = true
                    });
            });
        }

        public IServiceCollection ServiceCollection { get; } = new ServiceCollection();

        public override IServiceProvider ServiceProvider => _serviceProvider.Value;

        public override void Verify()
        {
            // ensure creation of lazy service provider, this will trigger the validation
            var unused = _serviceProvider.Value;
        }

        public override void Register(ServiceDescriptor serviceDescriptor)
        {
            if (_serviceProvider.IsValueCreated)
            {
                throw new InvalidOperationException("Service provider has been built and cannot be changed any more.");
            }

            var existingRegistration = ServiceCollection
                .SingleOrDefault(sd => sd.ServiceType == serviceDescriptor.ServiceType);

            if (existingRegistration == null)
            {
                serviceDescriptor.LogDetails(Logger, "Adding");
                ServiceCollection.Add(serviceDescriptor);
            }
            else
            {
                serviceDescriptor.LogDetails(Logger, "Replacing");
                ServiceCollection.Replace(serviceDescriptor);
            }
        }

        public override void RegisterDecorator(ServiceDescriptor serviceDescriptor)
        {
            serviceDescriptor.LogDetails(Logger, "Adding decorator");
            ServiceCollection.Decorate(serviceDescriptor.ServiceType, serviceDescriptor.ImplementationType);
        }

        public override void RegisterCollection(IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            var serviceDescriptorArray = serviceDescriptors as ServiceDescriptor[] ?? serviceDescriptors.ToArray();

            if (serviceDescriptorArray.Length == 0)
            {
                Logger.Warn("Skipping registration of empty collection");
                return;
            }

            foreach (var serviceDescriptor in serviceDescriptorArray)
            {
                serviceDescriptor.LogDetails(Logger, "Adding");
                ServiceCollection.Add(serviceDescriptor);
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