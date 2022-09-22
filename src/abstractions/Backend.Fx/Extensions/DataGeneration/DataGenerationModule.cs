using System.Linq;
using System.Reflection;
using Backend.Fx.DependencyInjection;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Extensions.DataGeneration
{
    public class DataGenerationModule : IModule
    {
        private readonly Assembly[] _assemblies;

        public DataGenerationModule(Assembly[] assemblies)
        {
            _assemblies = assemblies;
        }

        public void Register(ICompositionRoot compositionRoot)
        {
            compositionRoot.RegisterCollection(
                _assemblies
                    .GetImplementingTypes(typeof(IDataGenerator))
                    .Select(t => new ServiceDescriptor(typeof(IDataGenerator), t, ServiceLifetime.Scoped)));
        }
    }
}