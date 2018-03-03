namespace Backend.Fx.Testing.InMemoryPersistence
{
    using System.Reflection;
    using Bootstrapping.Modules;

    public class InMemoryDomainModule : DomainModule
    {
        public InMemoryDomainModule(params Assembly[] domainAssemblies) : base(domainAssemblies)
        { }
    }
}