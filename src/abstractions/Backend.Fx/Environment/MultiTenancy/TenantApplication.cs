using System;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DataGeneration;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Environment.MultiTenancy
{
    public abstract class TenantApplication
    {
        private static readonly ILogger Logger = Log.Create<TenantApplication>();

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
                Logger.LogInformation("Seeding data for recently activated tenant (with demo data: {IsDemoTenant}) {TenantId}",
                    tenantCreated.IsDemoTenant,
                    tenantCreated.TenantId);
                try
                {
                    _dataGenerationContext.SeedDataForTenant(new TenantId(tenantCreated.TenantId), tenantCreated.IsDemoTenant);
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