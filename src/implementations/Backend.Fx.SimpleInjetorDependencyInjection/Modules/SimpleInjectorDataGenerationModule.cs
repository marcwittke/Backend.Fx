using System;
using System.Reflection;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DataGeneration;
using Backend.Fx.Patterns.DependencyInjection;
using SimpleInjector;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Modules
{
    public class SimpleInjectorDataGenerationModule : IModule
    {
        private static readonly ILogger Logger = LogManager.Create<SimpleInjectorDataGenerationModule>();
        private readonly Assembly[] _domainAssemblies;

        public SimpleInjectorDataGenerationModule(params Assembly[] domainAssemblies)
        {
            _domainAssemblies = domainAssemblies;
        }
 
        public void Register(ICompositionRoot compositionRoot)
        {
            Container container = ((SimpleInjectorCompositionRoot) compositionRoot).Container;
            container.Collection.Register<IDataGenerator>(container.GetTypesToRegister(typeof(IDataGenerator), _domainAssemblies));
        }
    }
}