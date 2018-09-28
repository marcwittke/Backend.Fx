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

    /// <summary>
    /// Provides a tenant with initial data by running all available <see cref="InitialDataGenerator"/>s 
    /// </summary>
    public interface ITenantInitializer
    {
        /// <summary>
        /// Runs all <see cref="IProductiveDataGenerator"/>s inside the given <see cref="TenantId"/>
        /// </summary>
        /// <param name="tenantId"></param>
        void RunProductiveInitialDataGenerators(TenantId tenantId);

        /// <summary>
        /// Runs all <see cref="IDemoDataGenerator"/>s inside the given <see cref="TenantId"/>
        /// </summary>
        /// <param name="tenantId"></param>
        void RunDemoInitialDataGenerators(TenantId tenantId);
    }

    /// <inheritdoc />
    public class TenantInitializer : ITenantInitializer
    {
        private readonly IScopeManager _scopeManager;
        private static readonly ILogger Logger = LogManager.Create<TenantInitializer>();

        public TenantInitializer(IScopeManager scopeManager)
        {
            this._scopeManager = scopeManager;
        }

        /// <inheritdoc />
        public void RunProductiveInitialDataGenerators(TenantId tenantId)
        {
            Logger.Info("Loading productive data into database");
            RunInitialDataGeneratorsInSeparateScopes<IProductiveDataGenerator>(tenantId);
        }

        /// <inheritdoc />
        public void RunDemoInitialDataGenerators(TenantId tenantId)
        {
            Logger.Info("Loading demonstration data into database");
            RunInitialDataGeneratorsInSeparateScopes<IDemoDataGenerator>(tenantId);
        }

        /// <summary>
        /// Uses the dependency injection framework to find all <see cref="InitialDataGenerator"/>s and runs them according to the respective priority
        /// </summary>
        /// <typeparam name="TDataGenerator"></typeparam>
        /// <param name="tenantId"></param>
        private void RunInitialDataGeneratorsInSeparateScopes<TDataGenerator>(TenantId tenantId)
        {
            Queue<Type> dataGeneratorTypesToRun;
            using (var scope = _scopeManager.BeginScope(new SystemIdentity(), tenantId))
            {
                dataGeneratorTypesToRun = new Queue<Type>(scope.GetAllInstances<InitialDataGenerator>().OrderBy(idg => idg.Priority).OfType<TDataGenerator>().Select(idg => idg.GetType()));
            }

            while (dataGeneratorTypesToRun.Any())
            {
                var type = dataGeneratorTypesToRun.Dequeue();
                using (IScope scope = _scopeManager.BeginScope(new SystemIdentity(), tenantId))
                {
                    using (var unitOfWork = scope.GetInstance<IUnitOfWork>())
                    {
                        unitOfWork.Begin();
                        var instance = scope.GetAllInstances<InitialDataGenerator>().Single(idg => idg.GetType() == type);
                        instance.Generate();
                        unitOfWork.Complete();
                    }
                }
            }
        }
    }
}