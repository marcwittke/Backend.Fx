using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Exceptions;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.Jobs;

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
        protected BackendFxApplication(ICompositionRoot compositionRoot, IScopeManager scopeManager, ITenantManager tenantManager)
        {
            CompositionRoot = compositionRoot;
            ScopeManager = scopeManager;
            TenantManager = tenantManager;
            JobEngine = new JobEngine(scopeManager);
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
            InitializeTenants();
            _isBooted.Set();
        }

        private void InitializeTenants()
        {
            foreach (var tenant in TenantManager.GetTenants())
            {
                InitializeTenant(tenant);
            }
        }

        private void InitializeTenant(Tenant tenant)
        {
            var tenantDataGenerator = new TenantDataGenerator(ScopeManager);
            switch (tenant.State)
            {
                case TenantState.Inactive:
                    throw new UnprocessableException($"Cannot initialize inactive Tenant[{tenant.Id}]");

                case TenantState.Active:
                case TenantState.Created:
                    tenant.State = TenantState.Initializing;
                    TenantManager.SaveTenant(tenant);

                    Logger.Info(
                        $"Initializing {(tenant.IsDemoTenant ? "demonstration" : "production")} tenant[{tenant.Id}] ({tenant.Name})");
                    tenantDataGenerator.RunProductiveDataGenerators(tenant);
                    if (tenant.IsDemoTenant)
                    {
                        tenantDataGenerator.RunDemoDataGenerators(tenant);
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
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}