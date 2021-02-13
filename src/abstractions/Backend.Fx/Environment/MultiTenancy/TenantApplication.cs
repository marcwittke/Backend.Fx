using System;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DataGeneration;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.Environment.MultiTenancy
{
    public abstract class TenantApplication
    {
        private static readonly ILogger Logger = LogManager.Create<TenantApplication>();

        private readonly IBackendFxApplication _application;
        private readonly DataGenerationContext _dataGenerationContext;


        protected TenantApplication(IBackendFxApplication application)
        {
            _application = application;
            _dataGenerationContext = new DataGenerationContext(_application.CompositionRoot, _application.Invoker);
        }
        
        protected void EnableDataGenerationForNewTenants()
        {
            _application.MessageBus.Subscribe(new DelegateIntegrationMessageHandler<TenantActivated>(tenantCreated =>
            {
                Logger.Info($"Seeding data for recently activated {(tenantCreated.IsDemoTenant ? "demo " : "")}tenant {tenantCreated.TenantId}");
                try
                {
                    _dataGenerationContext.SeedDataForTenant(new TenantId(tenantCreated.TenantId), tenantCreated.IsDemoTenant);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Seeding data for recently activated {(tenantCreated.IsDemoTenant ? "demo " : "")}tenant {tenantCreated.TenantId} failed.");
                }
            }));
        }
    }
}