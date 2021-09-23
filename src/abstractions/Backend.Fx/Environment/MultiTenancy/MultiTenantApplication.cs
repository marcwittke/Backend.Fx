using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.Environment.MultiTenancy
{
    public class MultiTenantApplication : TenantApplication, IBackendFxApplication
    {
        private readonly IBackendFxApplication _application;

        public MultiTenantApplication(IBackendFxApplication application) : base(application)
        {
            _application = application;
        }

        public void Dispose()
        {
            _application.Dispose();
        }

        public IBackendFxApplicationAsyncInvoker AsyncInvoker => _application.AsyncInvoker;

        public ICompositionRoot CompositionRoot => _application.CompositionRoot;

        public IBackendFxApplicationInvoker Invoker => _application.Invoker;

        public IMessageBus MessageBus => _application.MessageBus;

        public async Task BootAsync(CancellationToken cancellationToken = default)
        {
            EnableDataGenerationForNewTenants();

            await _application.BootAsync(cancellationToken);
        }
    }
}
