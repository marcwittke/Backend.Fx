using System;
using System.Linq;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Patterns.DataGeneration
{
    public interface IDataGenerationContext
    {
        void SeedDataForTenant(TenantId tenantId, bool isDemoTenant);
    }

    public class DataGenerationContext : IDataGenerationContext
    {
        private static readonly ILogger Logger = Log.Create<DataGenerationContext>();

        private readonly ICompositionRoot _compositionRoot;
        private readonly IBackendFxApplicationInvoker _invoker;
        private readonly ITenantWideMutexManager _mutexManager;

        public DataGenerationContext(
            ICompositionRoot compositionRoot,
            IBackendFxApplicationInvoker invoker,
            ITenantWideMutexManager mutexManager)
        {
            _compositionRoot = compositionRoot;
            _invoker = invoker;
            _mutexManager = mutexManager;
        }

        public void SeedDataForTenant(TenantId tenantId, bool isDemoTenant)
        {
            if (!_mutexManager.TryAcquire(tenantId, GetType().Name, out var mutex)) return;
            
            using (mutex)
            {
                using (Logger.LogInformationDuration($"Seeding data for tenant {tenantId.Value}"))
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