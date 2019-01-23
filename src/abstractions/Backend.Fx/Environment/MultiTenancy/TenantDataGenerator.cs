namespace Backend.Fx.Environment.MultiTenancy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Authentication;
    using Logging;
    using Patterns.DataGeneration;
    using Patterns.DependencyInjection;
    using Patterns.UnitOfWork;

    public class TenantDataGenerator
    {
        private readonly IScopeManager _scopeManager;
        private static readonly ILogger Logger = LogManager.Create<TenantDataGenerator>();

        public TenantDataGenerator(IScopeManager scopeManager)
        {
            _scopeManager = scopeManager;
        }
        
        public void RunProductiveDataGenerators(Tenant tenant)
        {
            Logger.Info("Loading productive data into database");
            RunDataGeneratorsInSeparateScopes<IProductiveDataGenerator>(tenant);
        }

        public void RunDemoDataGenerators(Tenant tenant)
        {
            Logger.Info("Loading demonstration data into database");
            RunDataGeneratorsInSeparateScopes<IDemoDataGenerator>(tenant);
        }

        /// <summary>
        /// Uses the dependency injection framework to find all <see cref="DataGenerator"/>s and runs them according to the respective priority
        /// </summary>
        /// <typeparam name="TDataGenerator"></typeparam>
        /// <param name="tenant"></param>
        private void RunDataGeneratorsInSeparateScopes<TDataGenerator>(Tenant tenant)
        {
            Queue<Type> dataGeneratorTypesToRun;
            using (var scope = _scopeManager.BeginScope(new SystemIdentity(), new TenantId(tenant.Id)))
            {
                dataGeneratorTypesToRun = new Queue<Type>(scope.GetAllInstances<DataGenerator>().OrderBy(idg => idg.Priority).OfType<TDataGenerator>().Select(idg => idg.GetType()));
            }

            while (dataGeneratorTypesToRun.Any())
            {
                var type = dataGeneratorTypesToRun.Dequeue();
                using (IScope scope = _scopeManager.BeginScope(new SystemIdentity(), new TenantId(tenant.Id)))
                {
                    using (var unitOfWork = scope.GetInstance<IUnitOfWork>())
                    {
                        unitOfWork.Begin();
                        var instance = scope.GetAllInstances<DataGenerator>().Single(idg => idg.GetType() == type);
                        instance.Generate();
                        unitOfWork.Complete();
                    }
                }
            }
        }
    }
}