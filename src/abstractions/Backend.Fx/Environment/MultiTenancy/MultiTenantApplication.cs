using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DataGeneration;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Environment.MultiTenancy
{
    public class MultiTenantApplication : IBackendFxApplication
    {
        private static readonly ILogger Logger = Log.Create<MultiTenantApplication>();
        private readonly DataGenerationContext _dataGenerationContext;
        private readonly IBackendFxApplication _application;

        public MultiTenantApplication(IBackendFxApplication application)
        {
            _application = application;
            _dataGenerationContext = new DataGenerationContext(_application.CompositionRoot, _application.Invoker);
        }

        public void Dispose()
        {
            _application.Dispose();
        }

        public IBackendFxApplicationAsyncInvoker AsyncInvoker => _application.AsyncInvoker;

        public ICompositionRoot CompositionRoot => _application.CompositionRoot;

        public IBackendFxApplicationInvoker Invoker => _application.Invoker;

        public IMessageBus MessageBus => _application.MessageBus;

        public bool WaitForBoot(int timeoutMilliSeconds = Int32.MaxValue, CancellationToken cancellationToken = default)
        {
            return _application.WaitForBoot(timeoutMilliSeconds, cancellationToken);
        }

        public Task Boot(CancellationToken cancellationToken = default) => BootAsync(cancellationToken);

        public async Task BootAsync(CancellationToken cancellationToken = default)
        {
            EnableDataGenerationForNewTenants();

            await _application.BootAsync(cancellationToken);
        }

        private void EnableDataGenerationForNewTenants()
        {
            _application.MessageBus.Subscribe(new DelegateIntegrationMessageHandler<TenantActivated>(tenantCreated =>
            {
                Logger.LogInformation(
                    "Seeding data for recently activated tenant (with demo data: {IsDemoTenant}) {TenantId}",
                    tenantCreated.IsDemoTenant,
                    tenantCreated.TenantId);
                try
                {
                    _dataGenerationContext.SeedDataForTenant(new TenantId(tenantCreated.TenantId),
                        tenantCreated.IsDemoTenant);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex,
                        "Seeding data for recently activated tenant (with demo data: {IsDemoTenant}) {TenantId} failed",
                        tenantCreated.IsDemoTenant,
                        tenantCreated.TenantId);
                }
            }));
        }
    }
}