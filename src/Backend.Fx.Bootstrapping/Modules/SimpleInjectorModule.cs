namespace Backend.Fx.Bootstrapping.Modules
{
    using Patterns.DependencyInjection;
    using SimpleInjector;

    public abstract class SimpleInjectorModule : IModule
    {
        private readonly Container container;
        private readonly ScopedLifestyle scopedLifestyle;

        protected SimpleInjectorModule(SimpleInjectorCompositionRoot compositionRoot)
        {
            container = compositionRoot.Container;
            scopedLifestyle = compositionRoot.ScopedLifestyle;
        }

        public void Register()
        {
            Register(container, scopedLifestyle);
        }

        protected abstract void Register(Container container, ScopedLifestyle scopedLifestyle);
    }
}
