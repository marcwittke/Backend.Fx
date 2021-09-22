using System.Reflection;
using Backend.Fx.Patterns.DataGeneration;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Modules
{
    public class SimpleInjectorDataGenerationModule : IModule
    {
        private readonly Assembly[] _domainAssemblies;

        public SimpleInjectorDataGenerationModule(params Assembly[] domainAssemblies)
        {
            _domainAssemblies = domainAssemblies;
        }

        public void Register(ICompositionRoot compositionRoot)
        {
            var container = ((SimpleInjectorCompositionRoot)compositionRoot).Container;
            container.Collection.Register<IDataGenerator>(
                container.GetTypesToRegister(typeof(IDataGenerator), _domainAssemblies));
        }
    }
}
