using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Patterns.DataGeneration
{
    public interface IDataGenerationContext
    {
        void SeedDataForTenant(TenantId tenantId, bool isDemoTenant);
    }


    public class DataGenerationContext : IDataGenerationContext
    {
        private static readonly ILogger Logger = LogManager.Create<DataGenerationContext>();

        private readonly ICompositionRoot _compositionRoot;
        private readonly IBackendFxApplicationInvoker _invoker;

        public DataGenerationContext(ICompositionRoot compositionRoot, IBackendFxApplicationInvoker invoker)
        {
            _compositionRoot = compositionRoot;
            _invoker = invoker;
        }

        public void SeedDataForTenant(TenantId tenantId, bool isDemoTenant)
        {
            using (Logger.InfoDuration($"Seeding data for tenant {tenantId.Value}"))
            {
                Type[] dataGeneratorTypesToRun = GetDataGeneratorTypes(_compositionRoot, isDemoTenant);
                foreach (var dataGeneratorTypeToRun in dataGeneratorTypesToRun)
                {
                    _invoker.Invoke(
                        instanceProvider =>
                        {
                            var dataGenerator = instanceProvider
                                .GetInstances<IDataGenerator>()
                                .Single(dg => dg.GetType() == dataGeneratorTypeToRun);
                            dataGenerator.Generate();
                        },
                        new SystemIdentity(),
                        tenantId);
                }
            }
        }

        private static Type[] GetDataGeneratorTypes(ICompositionRoot compositionRoot, bool includeDemoDataGenerators)
        {
            using (var scope = compositionRoot.BeginScope())
            {
                IEnumerable<Type> dataGenerators = scope
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
