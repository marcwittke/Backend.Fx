using System;
using System.Linq;
using System.Reflection;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DataGeneration;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.Environment.MultiTenancy
{
    public static class BackendFxApplicationTenantExtensions
    {
        private static readonly ILogger Logger = LogManager.Create(typeof(BackendFxApplicationTenantExtensions));

        public static void RegisterSeedActionForNewlyCreatedTenants(this IBackendFxApplication application, ITenantService tenantService)
        {
            application.CompositionRoot
                .GetInstance<IEventBus>()
                .Subscribe(new DelegateIntegrationEventHandler<TenantCreated>(tenantCreated =>
                {
                    Logger.Info($"Seeding data for recently created {(tenantCreated.IsDemoTenant?"demo ":"")}tenant {tenantCreated.TenantId}");
                    try
                    {
                        var tenantId = new TenantId(tenantCreated.TenantId);
                        application.SeedDataForTenant(tenantId, tenantCreated.IsDemoTenant);
                        tenantService.ActivateTenant(tenantId);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, $"Seeding data for recently created {(tenantCreated.IsDemoTenant ? "demo " : "")}tenant {tenantCreated.TenantId} failed.");
                    }
                }));
        }

        public static void SeedDataForAllActiveTenants(this IBackendFxApplication application)
        {
            using (Logger.InfoDuration("Seeding data"))
            {
                var prodTenantIds = application.TenantIdService.GetActiveProductionTenantIds();
                foreach (var prodTenantId in prodTenantIds)
                {
                    application.SeedDataForTenant(prodTenantId, false);
                }

                var demoTenantIds = application.TenantIdService.GetActiveDemonstrationTenantIds();
                foreach (var demoTenantId in demoTenantIds)
                {
                    application.SeedDataForTenant(demoTenantId, true);
                }
            }
        }

        private static void SeedDataForTenant(this IBackendFxApplication application, TenantId tenantId, bool isDemoTenant)
        {
            using (Logger.InfoDuration($"Seeding data for tenant {tenantId.Value}"))
            {
                Type[] dataGeneratorTypesToRun;

                using (application.BeginScope())
                {
                    var dataGenerators = application.CompositionRoot.GetInstances<IDataGenerator>()
                        .OrderBy(dg => dg.Priority)
                        .Select(dg => dg.GetType());

                    if (!isDemoTenant)
                    {
                        dataGenerators = dataGenerators.Where(dg => !typeof(IDemoDataGenerator).IsAssignableFrom(dg));
                    }

                    dataGeneratorTypesToRun = dataGenerators.ToArray();
                }

                foreach (var dataGeneratorTypeToRun in dataGeneratorTypesToRun)
                {
                    application.InvokeAsync(() =>
                    {
                        IDataGenerator dataGenerator = application
                            .CompositionRoot
                            .GetInstances<IDataGenerator>()
                            .Single(dg => dg.GetType() == dataGeneratorTypeToRun);
                        dataGenerator.Generate();
                    }, new SystemIdentity(), tenantId);
                }
            }
        }
    }
}
