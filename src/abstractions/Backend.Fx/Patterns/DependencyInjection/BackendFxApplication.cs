using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Extensions;
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
        private int _scopeIndex = 1;
        
        /// <summary>
        /// Initializes the application's runtime instance
        /// </summary>
        /// <param name="compositionRoot">The composition root of the dependency injection framework</param>
        /// <param name="tenantManager">The tenant manager for the current appliastion</param>
        /// <param name="exceptionLogger">The exception logger used for job execution and integration event handling</param>
        protected BackendFxApplication(ICompositionRoot compositionRoot, ITenantManager tenantManager, IExceptionLogger exceptionLogger)
        {
            CompositionRoot = compositionRoot;
            TenantManager = tenantManager;
            ExceptionLogger = exceptionLogger;
            JobEngine = new JobEngine(this);
        }

        /// <inheritdoc />
        public ITenantManager TenantManager { get; }

        public IExceptionLogger ExceptionLogger { get; }

        /// <inheritdoc />
        public ICompositionRoot CompositionRoot { get; }

        public IJobEngine JobEngine { get; }

        /// <inheritdoc />
        public async Task Boot()
        {
            Logger.Info("Booting application");
            await OnBoot();
            CompositionRoot.Verify();
            new DataGeneratorContext(this).SeedDataForAllActiveTenants();

            TenantManager.TenantCreated += (sender, tenantId) =>
            {
                Task.Run(() =>
                {
                    try
                    {
                        var tenant = TenantManager.GetTenant(tenantId);
                        tenant.State = TenantState.Seeding;
                        TenantManager.SaveTenant(tenant);
                        new DataGeneratorContext(this).SeedDataForTenant(tenant);
                        tenant.State = TenantState.Active;
                        TenantManager.SaveTenant(tenant);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Handling TenantCreated event failed");
                    }
                });
            };

            await OnBooted();
            _isBooted.Set();
        }

        public Task<bool> WaitForBootAsync(int timeoutMilliSeconds = int.MaxValue)
        {
            return Task.Run(() => WaitForBoot(timeoutMilliSeconds));
        }

        public bool WaitForBoot(int timeoutMilliSeconds = int.MaxValue)
        {
            return _isBooted.Wait(timeoutMilliSeconds);
        }

        public IDisposable BeginScope(IIdentity identity = null, TenantId tenantId = null)
        {
            var scopeIndex = _scopeIndex++;
            tenantId = tenantId ?? new TenantId(null);
            identity = identity ?? new AnonymousIdentity();

            var scopeDurationLogger = Logger.InfoDuration(
                $"Starting scope {scopeIndex} for {identity.Name} in tenant {(tenantId.HasValue ? tenantId.Value.ToString() : "null")}",
                $"Ended scope {scopeIndex} for {identity.Name} in tenant {(tenantId.HasValue ? tenantId.Value.ToString() : "null")}");
            var scope = CompositionRoot.BeginScope();
            CompositionRoot.GetInstance<ICurrentTHolder<TenantId>>().ReplaceCurrent(tenantId);
            CompositionRoot.GetInstance<ICurrentTHolder<IIdentity>>().ReplaceCurrent(identity);

            return new MultipleDisposable(scope, scopeDurationLogger);
        }

        public Task InvokeAsync(Action action, IIdentity identity, TenantId tenantId)
        {
            return Task.Run(() => Invoke(action, identity, tenantId));
        }

        public void Invoke(Action action, IIdentity identity, TenantId tenantId)
        {
            using (BeginScope(new SystemIdentity(), tenantId))
            {
                using (var unitOfWork = CompositionRoot.GetInstance<IUnitOfWork>())
                {
                    try
                    {
                        unitOfWork.Begin();
                        action.Invoke();
                        unitOfWork.Complete();
                    }
                    catch (Exception ex)
                    {
                        Logger.Info(ex);
                        ExceptionLogger.LogException(ex);
                    }
                }
            }
        }
        
        /// <summary>
        /// Extension point to do additional initialization before composition root is initialized
        /// </summary>
        /// <returns></returns>
        protected virtual async Task OnBoot()
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Extension point to do additional initialization after composition root is initialized
        /// </summary>
        /// <returns></returns>
        protected virtual async Task OnBooted()
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