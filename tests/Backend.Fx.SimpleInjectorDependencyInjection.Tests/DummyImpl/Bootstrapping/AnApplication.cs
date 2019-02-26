using Backend.Fx.InMemoryPersistence;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Bootstrapping
{
    public class AnApplication : BackendFxApplication
    {
        private static readonly ILogger Logger = LogManager.Create<AnApplication>();

        public AnApplication(ICompositionRoot compositionRoot)
            : base(compositionRoot, new InMemoryTenantManager(), new ExceptionLogger(Logger))
        {}

    }
}