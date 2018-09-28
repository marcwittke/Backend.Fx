using Backend.Fx.SimpleInjectorDependencyInjection.Modules;

namespace Backend.Fx.Testing.InMemoryPersistence
{
    using System.Reflection;

    public class InMemoryDomainModule : DomainModule
    {
        public InMemoryDomainModule(params Assembly[] domainAssemblies) : base(domainAssemblies)
        { }
    }
}