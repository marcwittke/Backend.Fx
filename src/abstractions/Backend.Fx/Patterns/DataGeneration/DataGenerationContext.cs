using System;
using System.Linq;
using System.Reflection;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Patterns.DataGeneration
{
    public class DataGenerationContext
    {
        private static readonly ILogger Logger = LogManager.Create<DataGenerationContext>();

        private readonly ITenantService _tenantService;
        private readonly ICompositionRoot _compositionRoot;
        private readonly IBackendFxApplicationInvoker _invoker;

        public DataGenerationContext(ITenantService tenantService, ICompositionRoot compositionRoot, IBackendFxApplicationInvoker invoker)
        {
            _tenantService = tenantService;
            _compositionRoot = compositionRoot;
            _invoker = invoker;
        }

        public void SeedDataForAllActiveTenants()
        {
            using (Logger.InfoDuration("Seeding data"))
            {
                var prodTenantIds = _tenantService.GetActiveProductionTenantIds();
                foreach (TenantId prodTenantId in prodTenantIds)
                {
                    SeedDataForTenant(prodTenantId, false);
                }

                var demoTenantIds = _tenantService.GetActiveDemonstrationTenantIds();
                foreach (TenantId demoTenantId in demoTenantIds)
                {
                    SeedDataForTenant(demoTenantId, true);
                }
            }
        }

        public void SeedDataForTenant(TenantId tenantId, bool isDemoTenant)
        {
            using (Logger.InfoDuration($"Seeding data for tenant {tenantId.Value}"))
            {
                Type[] dataGeneratorTypesToRun = GetDataGeneratorTypes(_compositionRoot, isDemoTenant);
                foreach (Type dataGeneratorTypeToRun in dataGeneratorTypesToRun)
                {
                    _invoker.Invoke(instanceProvider =>
                    {
                        IDataGenerator dataGenerator = instanceProvider
                                                       .GetInstances<IDataGenerator>()
                                                       .Single(dg => dg.GetType() == dataGeneratorTypeToRun);
                        dataGenerator.Generate();
                    }, new SystemIdentity(), tenantId);
                }
            }
        }

        private static Type[] GetDataGeneratorTypes(ICompositionRoot compositionRoot, bool includeDemoDataGenerators)
        {
            using (IInjectionScope scope = compositionRoot.BeginScope())
            {
                var dataGenerators = scope
                                     .InstanceProvider
                                     .GetInstances<IDataGenerator>()
                                     .OrderBy(dg => dg.Priority)
                                     .Select(dg => dg.GetType());

                if (!includeDemoDataGenerators)
                {
                    dataGenerators = dataGenerators.Where(dg => !typeof(IDemoDataGenerator).IsAssignableFrom(dg));
                }

                return dataGenerators.ToArray();
            }
        }
    }
}