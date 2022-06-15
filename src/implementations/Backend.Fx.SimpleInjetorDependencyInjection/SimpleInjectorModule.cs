using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleInjector;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.SimpleInjectorDependencyInjection
{
    public abstract class SimpleInjectorModule : IModule
    {
        private static readonly ILogger Logger = Log.Create<SimpleInjectorModule>();

        public virtual void Register(ICompositionRoot compositionRoot)
        {
            Logger.LogDebug("Registering {Module}", GetType().Name);
            var simpleInjectorCompositionRoot = (SimpleInjectorCompositionRoot) compositionRoot;
            Register(simpleInjectorCompositionRoot.Container, simpleInjectorCompositionRoot.ScopedLifestyle);
        }
        
        protected abstract void Register(Container container, ScopedLifestyle scopedLifestyle);
    }
}