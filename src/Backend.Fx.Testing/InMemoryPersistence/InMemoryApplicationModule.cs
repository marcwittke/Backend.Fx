namespace Backend.Fx.Testing.InMemoryPersistence
{
    using System.Reflection;
    using Bootstrapping;
    using Bootstrapping.Modules;

    public class InMemoryApplicationModule : ApplicationModule
    {
        public InMemoryApplicationModule(SimpleInjectorCompositionRoot compositionRoot, params Assembly[] domainAssemblies) : base(compositionRoot, domainAssemblies)
        { }
    }
}