using System;
using System.Collections.Generic;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.MicrosoftDependencyInjection
{
    public class MicrosoftCompositionRoot : ICompositionRoot
    {
        private readonly IServiceCollection _serviceCollection = new ServiceCollection();
        private readonly Lazy<IServiceProvider> _serviceProvider;
        public MicrosoftCompositionRoot()
        {
            _serviceProvider = new Lazy<IServiceProvider>(() => _serviceCollection.BuildServiceProvider());
        }
        public void Dispose()
        {
        }

        public void Verify()
        { }

        public void RegisterModules(params IModule[] modules)
        {
            foreach (var module in modules)
            {
                module.Register(this);
            }
        }

        public void RegisterServiceDescriptor(ServiceDescriptor serviceDescriptor)
        {
            if (_serviceProvider.IsValueCreated)
            {
                throw new InvalidOperationException("Service provider has been built and cannot be changed any more.");
            }
            _serviceCollection.Add(serviceDescriptor);
        }

        public void RegisterServiceDescriptors(IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            foreach (var serviceDescriptor in serviceDescriptors)
            {
                RegisterServiceDescriptor(serviceDescriptor);    
            }
        }

        public IServiceScope BeginScope()
        {
            return ServiceProvider.CreateScope();
        }

        public IServiceProvider ServiceProvider => _serviceProvider.Value;
    }
}