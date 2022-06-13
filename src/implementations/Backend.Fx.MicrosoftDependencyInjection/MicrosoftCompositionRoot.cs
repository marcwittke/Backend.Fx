using System;
using System.Collections.Generic;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.MicrosoftDependencyInjection
{
    public class MicrosoftCompositionRoot : CompositionRoot
    {
        private readonly IServiceCollection _serviceCollection = new ServiceCollection();
        private readonly Lazy<IServiceProvider> _serviceProvider;
        
        public MicrosoftCompositionRoot()
        {
            _serviceProvider = new Lazy<IServiceProvider>(() => _serviceCollection.BuildServiceProvider());
        }

        public override IServiceProvider ServiceProvider => _serviceProvider.Value;
        
        public override void Verify()
        { }

        public override void RegisterServiceDescriptor(ServiceDescriptor serviceDescriptor)
        {
            if (_serviceProvider.IsValueCreated)
            {
                throw new InvalidOperationException("Service provider has been built and cannot be changed any more.");
            }
            _serviceCollection.Add(serviceDescriptor);
        }

        public override void RegisterServiceDescriptors(IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            foreach (var serviceDescriptor in serviceDescriptors)
            {
                RegisterServiceDescriptor(serviceDescriptor);    
            }
        }

        public override IServiceScope BeginScope()
        {
            return ServiceProvider.CreateScope();
        }

        protected override void Dispose(bool disposing)
        { }
    }
}