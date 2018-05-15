namespace Backend.Fx.Bootstrapping.Modules
{
    using Logging;
    using Patterns.DependencyInjection;
    using SimpleInjector;

    public abstract class SimpleInjectorModule : IModule
    {
        private static readonly ILogger Logger = LogManager.Create<SimpleInjectorModule>();

        protected abstract void Register(Container container, ScopedLifestyle scopedLifestyle);

        public virtual void Register(ICompositionRoot compositionRoot)
        {
            Logger.Debug($"Registering {GetType().Name}");
            SimpleInjectorCompositionRoot simpleInjectorCompositionRoot = (SimpleInjectorCompositionRoot) compositionRoot;
            Register(simpleInjectorCompositionRoot.Container, simpleInjectorCompositionRoot.ScopedLifestyle);
        }
    }
}
