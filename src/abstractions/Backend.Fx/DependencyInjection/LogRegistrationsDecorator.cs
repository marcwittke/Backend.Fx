using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Fx.Logging;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.DependencyInjection
{
    public class LogRegistrationsDecorator : ICompositionRoot
    {
        private readonly ILogger _logger = Log.Create<CompositionRoot>();
        private readonly ICompositionRoot _compositionRoot;

        public LogRegistrationsDecorator(ICompositionRoot compositionRoot)
        {
            _compositionRoot = compositionRoot;
        }

        public void Dispose()
        {
            _compositionRoot.Dispose();
        }

        public void Verify()
        {
            _compositionRoot.Verify();
        }

        public void RegisterModules(params IModule[] modules)
        {
            _compositionRoot.RegisterModules(modules);
        }

        public void Register(ServiceDescriptor serviceDescriptor)
        {
            LogDetails("Adding", "registration", serviceDescriptor);
            _compositionRoot.Register(serviceDescriptor);
        }

        public void RegisterDecorator(ServiceDescriptor serviceDescriptor)
        {
            LogDetails("Adding", "decorator", serviceDescriptor);
            _compositionRoot.RegisterDecorator(serviceDescriptor);
        }

        public void RegisterCollection(IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            serviceDescriptors = serviceDescriptors as ServiceDescriptor[] ?? serviceDescriptors.ToArray();
            LogAddCollectionRegistration(serviceDescriptors);
            if (serviceDescriptors.GroupBy(sd => sd.ServiceType).Count() > 1)
            {
                _logger.LogError("Attempt to register a collection of services for different service types");
            }
            _compositionRoot.RegisterCollection(serviceDescriptors);
        }

        private void LogAddCollectionRegistration(IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            serviceDescriptors = serviceDescriptors as ServiceDescriptor[] ?? serviceDescriptors.ToArray();
            _logger.LogDebug("{Verb} {Lifetime} {RegistrationType} for {ServiceType}: {ImplementationType}",
                "Adding",
                serviceDescriptors.First().Lifetime.ToString().ToLowerInvariant(),
                "collection registration",
                serviceDescriptors.First().ServiceType.GetDetailedTypeName(),
                string.Join(", ", serviceDescriptors.Select(sd => sd.GetImplementationTypeDescription())));
        }

        public IServiceScope BeginScope()
        {
            return _compositionRoot.BeginScope();
        }

        public IServiceProvider ServiceProvider => _compositionRoot.ServiceProvider;


        private void LogDetails(string verb, string registrationType, ServiceDescriptor serviceDescriptor)
        {
            _logger.LogDebug("{Verb} {Lifetime} {RegistrationType} for {ServiceType}: {ImplementationType}",
                verb,
                serviceDescriptor.Lifetime.ToString().ToLowerInvariant(),
                registrationType,
                serviceDescriptor.ServiceType.GetDetailedTypeName(),
                serviceDescriptor.GetImplementationTypeDescription());
        }
    }
}