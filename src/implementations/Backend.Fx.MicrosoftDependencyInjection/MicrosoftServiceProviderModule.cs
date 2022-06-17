using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.MicrosoftDependencyInjection
{
    public abstract class MicrosoftServiceProviderModule : IModule
    {
        private static readonly ILogger Logger = Log.Create<MicrosoftServiceProviderModule>();
        
        public virtual void Register(ICompositionRoot compositionRoot)
        {
            Logger.LogDebug("Registering {Module}", GetType().Name);
            var microsoftCompositionRoot = (MicrosoftCompositionRoot) compositionRoot;
            Register(microsoftCompositionRoot.ServiceCollection);
        }
        
        protected abstract void Register(IServiceCollection serviceCollection);
    }
}