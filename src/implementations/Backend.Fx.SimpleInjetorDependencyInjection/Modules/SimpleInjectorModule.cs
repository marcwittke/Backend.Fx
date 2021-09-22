using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using SimpleInjector;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Modules
{
    public abstract class SimpleInjectorModule : IModule
    {
        private static readonly ILogger Logger = LogManager.Create<SimpleInjectorModule>();

        public virtual void Register(ICompositionRoot compositionRoot)
        {
            Logger.Debug($"Registering {GetType().Name}");
            var simpleInjectorCompositionRoot = (SimpleInjectorCompositionRoot)compositionRoot;
            Register(simpleInjectorCompositionRoot.Container, simpleInjectorCompositionRoot.ScopedLifestyle);
        }

        protected abstract void Register(Container container, ScopedLifestyle scopedLifestyle);
    }
}
