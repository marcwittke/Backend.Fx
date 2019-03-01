using System;
using System.Linq;
using System.Reflection;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Patterns.DataGeneration
{
    public class DataGeneratorContext
    {
        private static readonly ILogger Logger = LogManager.Create<DataGeneratorContext>();
        private readonly IBackendFxApplication _application;
        
        public DataGeneratorContext(IBackendFxApplication application)
        {
            _application = application;
        }

        public void SeedDataForAllActiveTenants()
        {
            using (Logger.InfoDuration("Seeding data"))
            {
                var tenants = _application.TenantManager.GetTenants();
                foreach (var tenant in tenants)
                {
                    if (tenant.State == TenantState.Active)
                    {
                        SeedDataForTenant(tenant);
                    }
                    else
                    {
                        Logger.Info($"Not seeding data for tenant {tenant.Id} because it is not active but {tenant.State}");
                    }
                }
            }
        }

        public void SeedDataForTenant(Tenant tenant)
        {
            using (Logger.InfoDuration($"Seeding data for tenant {tenant.Id} ({tenant.Name})"))
            {
                Type[] dataGeneratorTypesToRun;

                using (_application.CompositionRoot.BeginScope())
                {
                    var dataGenerators = _application.CompositionRoot.GetInstances<IDataGenerator>()
                        .OrderBy(dg => dg.Priority)
                        .Select(dg => dg.GetType());

                    if (!tenant.IsDemoTenant)
                    {
                        dataGenerators = dataGenerators.Where(dg => !typeof(IDemoDataGenerator).IsAssignableFrom(dg));
                    }

                    dataGeneratorTypesToRun = dataGenerators.ToArray();
                }

                foreach (var dataGeneratorTypeToRun in dataGeneratorTypesToRun)
                {
                    _application.Invoke(() =>
                    {
                        IDataGenerator dataGenerator = _application
                            .CompositionRoot
                            .GetInstances<IDataGenerator>()
                            .Single(dg => dg.GetType() == dataGeneratorTypeToRun);
                        dataGenerator.Generate();
                    }, new SystemIdentity(), new TenantId(tenant.Id));
                }
            }
        }
    }
}
