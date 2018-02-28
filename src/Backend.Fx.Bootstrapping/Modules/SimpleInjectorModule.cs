namespace Backend.Fx.Bootstrapping.Modules
{
    using Patterns.DependencyInjection;
    using SimpleInjector;

    public abstract class SimpleInjectorModule : IModule
    {
        protected abstract void Register(Container container, ScopedLifestyle scopedLifestyle);

        public virtual void Register(ICompositionRoot compositionRoot)
        {
            SimpleInjectorCompositionRoot simpleInjectorCompositionRoot = (SimpleInjectorCompositionRoot) compositionRoot;
            Register(simpleInjectorCompositionRoot.Container, simpleInjectorCompositionRoot.ScopedLifestyle);
        }
    }
}
