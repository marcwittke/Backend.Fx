using Backend.Fx.DependencyInjection;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SimpleInjector;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.SimpleInjectorDependencyInjection
{
    [PublicAPI]
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