using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Exceptions;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DataGeneration;
using Backend.Fx.Patterns.Jobs;
using Backend.Fx.Patterns.UnitOfWork;

namespace Backend.Fx.Patterns.DependencyInjection
{
    /// <summary>
    /// The root object of the whole backend fx application framework
    /// </summary>
    public abstract class BackendFxApplication : IBackendFxApplication
    {
        private static readonly ILogger Logger = LogManager.Create<BackendFxApplication>();
        private readonly ManualResetEventSlim _isBooted = new ManualResetEventSlim(false);

        /// <summary>
        /// Initializes the application's runtime instance
        /// </summary>
        /// <param name="compositionRoot">The composition root of the dependency injection framework</param>
        /// <param name="scopeManager">The scope manager for the current application</param>
        /// <param name="tenantManager">The tenant manager for the current appliastion</param>
        protected BackendFxApplication(ICompositionRoot compositionRoot, IScopeManager scopeManager,
            ITenantManager tenantManager)
        {
            CompositionRoot = compositionRoot;
            ScopeManager = scopeManager;
            JobEngine = new JobEngine(scopeManager);
            TenantManager = tenantManager;
        }

        /// <inheritdoc />
        public IScopeManager ScopeManager { get; }

        /// <inheritdoc />
        public IJobEngine JobEngine { get; }

        public Task<bool> WaitForBootAsync(int timeoutMilliSeconds = int.MaxValue)
        {
            return Task.Run(() => WaitForBoot(timeoutMilliSeconds));
        }

        public bool WaitForBoot(int timeoutMilliSeconds = int.MaxValue)
        {
            return _isBooted.Wait(timeoutMilliSeconds);
        }

        /// <inheritdoc />
        public ITenantManager TenantManager { get; }

        /// <inheritdoc />
        public ICompositionRoot CompositionRoot { get; }

        /// <inheritdoc />
        public async Task Boot()
        {
            Logger.Info("Booting application");
            CompositionRoot.Verify();
            await OnBoot();
            SeedTenants();

            TenantManager.TenantCreated += async (sender, tenantId) =>
            {
                try
                {
                    if (TenantManager.GetTenant(tenantId).IsDemoTenant)
                    {
                        await JobEngine.ExecuteJobAsync<DemoDataGenerationJob>(tenantId);
                    }
                    else
                    {
                        await JobEngine.ExecuteJobAsync<ProdDataGenerationJob>(tenantId);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Handling TenantCreated event failed");
                }
            };

            _isBooted.Set();
        }

        private void SeedTenants()
        {
            Logger.Info("Beginning startup seeding");
            foreach (var tenant in TenantManager.GetTenantIds())
            {
                Logger.Debug($"Startup seeding of tenant[{tenant.Value}]");
                SeedTenant(tenant);
            }
        }

        private void SeedTenant(TenantId tenantId)
        {
            var tenant = TenantManager.GetTenant(tenantId);

            switch (tenant.State)
            {
                case TenantState.Inactive:
                    throw new UnprocessableException($"Cannot seed inactive Tenant[{tenant.Id}]");

                case TenantState.Active:
                case TenantState.Created:
                    tenant.State = TenantState.Seeding;
                    TenantManager.SaveTenant(tenant);
                    Logger.Info($"Seeding {(tenant.IsDemoTenant ? "demonstration" : "production")} tenant[{tenant.Id}] ({tenant.Name})");
                    using (var scope = ScopeManager.BeginScope(new SystemIdentity(), tenantId))
                    {
                        var dataGeneratorContext = new DataGeneratorContext(scope.GetAllInstances<IDataGenerator>(), scope.GetInstance<ICanFlush>());
                        dataGeneratorContext.RunProductiveDataGenerators();
                        if (tenant.IsDemoTenant)
                        {
                            dataGeneratorContext.RunDemoDataGenerators();
                        }
                    }

                    tenant.State = TenantState.Active;
                    return;

                default:
                    return;
            }
        }
        
        /// <summary>
        /// Extension point to do additional initialization after composition root is initialized
        /// </summary>
        /// <returns></returns>
        protected virtual async Task OnBoot()
        {
            await Task.CompletedTask;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Logger.Info("Application shut down initialized");
                CompositionRoot?.Dispose();
                TenantManager?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}