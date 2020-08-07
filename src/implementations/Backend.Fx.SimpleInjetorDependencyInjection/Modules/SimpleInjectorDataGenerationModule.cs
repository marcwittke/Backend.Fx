using System;
using System.Reflection;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DataGeneration;
using Backend.Fx.Patterns.DependencyInjection;
using SimpleInjector;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Modules
{
    public class SimpleInjectorDataGenerationModule : DataGenerationModule
    {
        private static readonly ILogger Logger = LogManager.Create<SimpleInjectorDataGenerationModule>();
        private readonly Assembly[] _domainAssemblies;

        public SimpleInjectorDataGenerationModule(params Assembly[] domainAssemblies)
        {
            _domainAssemblies = domainAssemblies;
        }
        
        protected override void RegisterDataGenerators(ICompositionRoot compositionRoot)
        {
            Container container = ((SimpleInjectorCompositionRoot) compositionRoot).Container;
            // initial data generation subsystem
            foreach (Type dataGeneratorType in container.GetTypesToRegister(typeof(IDataGenerator), _domainAssemblies))
            {
                Logger.Debug($"Appending {dataGeneratorType.Name} to list of IDataGenerators");
                container.Collection.Append(typeof(IDataGenerator), dataGeneratorType);
            }
        }
    }
}