using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Fx.DependencyInjection;
using Backend.Fx.Logging;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.MicrosoftDependencyInjection
{
    public class MicrosoftCompositionRoot : CompositionRoot
    {
        private readonly ILogger _logger = Log.Create<MicrosoftCompositionRoot>();
        private readonly Lazy<IServiceProvider> _serviceProvider;

        public MicrosoftCompositionRoot()
        {
            _serviceProvider = new Lazy<IServiceProvider>(() =>
            {
                _logger.LogInformation("Building Microsoft ServiceProvider");
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
                ServiceCollection.Add(serviceDescriptor);
            }
            else
            {
                _logger.LogDebug("{Verb} {Lifetime} {RegistrationType} for {ServiceType}: {ImplementationType}",
                    "Replacing",
                    serviceDescriptor.Lifetime.ToString().ToLowerInvariant(),
                    "registration",
                    serviceDescriptor.ServiceType.GetDetailedTypeName(),
                    serviceDescriptor.GetImplementationTypeDescription());
                ServiceCollection.Replace(serviceDescriptor);
            }
        }

        public override void RegisterDecorator(ServiceDescriptor serviceDescriptor)
        {
            if (serviceDescriptor.ServiceType.IsOpenGeneric())
            {
                throw new NotSupportedException("Microsoft's DI does not support decoration of open generic types. " +
                                                "See https://github.com/khellang/Scrutor/issues/39 for more info");
            }

            if (ServiceCollection.Any(sd => sd.ServiceType == serviceDescriptor.ServiceType))
            {
                ServiceCollection.Decorate(serviceDescriptor.ServiceType, serviceDescriptor.ImplementationType);
            }
            else
            {
                _logger.LogWarning("Skipping registration of decorator {DecoratorTypeName} because the service type to decorate ({ServiceType}) is not registered",
                    serviceDescriptor.GetImplementationTypeDescription(),
                    serviceDescriptor.ServiceType.Name);
            }
        }

        public override void RegisterCollection(IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            var serviceDescriptorArray = serviceDescriptors as ServiceDescriptor[] ?? serviceDescriptors.ToArray();

            if (serviceDescriptorArray.Length == 0)
            {
                _logger.LogWarning("Skipping registration of empty collection");
                return;
            }

            foreach (var serviceDescriptor in serviceDescriptorArray)
            {
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