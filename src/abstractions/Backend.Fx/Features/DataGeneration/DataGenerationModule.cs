using System.Linq;
using System.Reflection;
using Backend.Fx.Extensions;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.DataGeneration
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