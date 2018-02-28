namespace Backend.Fx.Testing.InMemoryPersistence
{
    using System.Reflection;
    using Bootstrapping.Modules;

    public class InMemoryApplicationModule : ApplicationModule
    {
        public InMemoryApplicationModule(params Assembly[] domainAssemblies) : base(domainAssemblies)
        { }
    }
}