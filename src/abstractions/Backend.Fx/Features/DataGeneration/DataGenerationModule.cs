using System;
using System.Linq;
using System.Reflection;
using Backend.Fx.DependencyInjection;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.DataGeneration
{
    internal class DataGenerationModule : IModule
    {
        private readonly Assembly[] _assemblies;
        private readonly bool _allowDemoDataGeneration;

        public DataGenerationModule(Assembly[] assemblies, bool allowDemoDataGeneration)
        {
            _assemblies = assemblies;
            _allowDemoDataGeneration = allowDemoDataGeneration;
        }

        public void Register(ICompositionRoot compositionRoot)
        {
            Type serviceType = _allowDemoDataGeneration
                ? typeof(IDataGenerator)
                : typeof(IProductiveDataGenerator);

            compositionRoot.RegisterCollection(
                _assemblies
                    .GetImplementingTypes(serviceType)
                    .Select(t => new ServiceDescriptor(typeof(IDataGenerator), t, ServiceLifetime.Scoped)));
            
            compositionRoot.Register(
                ServiceDescriptor.Singleton<IDataGenerationContext, DataGenerationContext>());
        }
    }
    
    internal class MultiTenancyDataGenerationModule : IModule
    {
        public void Register(ICompositionRoot compositionRoot)
        {
            compositionRoot.RegisterDecorator(
                ServiceDescriptor.Singleton<IDataGenerationContext, ForEachTenantDataGenerationContext>());
        }
    }
}