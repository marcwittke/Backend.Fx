using System;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Extensions;
using Backend.Fx.Logging;
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
        /// <param name="tenantIdService">This service provides all known tenant ids</param>
        /// <param name="exceptionLogger">The exception logger used for job execution and integration event handling</param>
        protected BackendFxApplication(ICompositionRoot compositionRoot, ITenantIdService tenantIdService, IExceptionLogger exceptionLogger)
        {
            CompositionRoot = compositionRoot;
            TenantIdService = tenantIdService;
            ExceptionLogger = exceptionLogger;
        }

        public IExceptionLogger ExceptionLogger { get; }

        /// <inheritdoc />
        public ICompositionRoot CompositionRoot { get; }

        public ITenantIdService TenantIdService { get; }

        /// <inheritdoc />
        public async Task Boot(CancellationToken cancellationToken = default)
        {
            Logger.Info("Booting application");
            await OnBoot(cancellationToken);
            CompositionRoot.Verify();

            await OnBooted(cancellationToken);
            _isBooted.Set();
        }

        public bool WaitForBoot(int timeoutMilliSeconds = int.MaxValue, CancellationToken cancellationToken = default)
        {
            return _isBooted.Wait(timeoutMilliSeconds, cancellationToken);
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

        public void Run<TJob>() where TJob : class, IJob
        {
            var tenantIds = TenantIdService.GetActiveTenantIds();
            foreach (var tenantId in tenantIds)
            {
                Invoke(() => CompositionRoot.GetInstance<TJob>().Run(), new SystemIdentity(), tenantId);
            }
        }

        public void Run<TJob>(TenantId tenantId) where TJob : class, IJob
        {
            Invoke(() => CompositionRoot.GetInstance<TJob>().Run(), new SystemIdentity(), tenantId);
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
                    catch (TargetInvocationException ex)
                    {
                        ExceptionLogger.LogException(ex.InnerException ?? ex);
                    }
                    catch (Exception ex)
                    {
                        Logger.Info(ex);
                        ExceptionLogger.LogException(ex);
                    }
                }
            }
        }

        public async Task InvokeAsync(Action action, IIdentity identity, TenantId tenantId, CancellationToken cancellationToken = default)
        {
            using (BeginScope(new SystemIdentity(), tenantId))
            {
                using (var unitOfWork = CompositionRoot.GetInstance<IUnitOfWork>())
                {
                    try
                    {
                        unitOfWork.Begin();
                        await Task.Factory.StartNew(action, cancellationToken);
                        unitOfWork.Complete();
                    }
                    catch (TargetInvocationException ex)
                    {
                        ExceptionLogger.LogException(ex.InnerException ?? ex);
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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected virtual async Task OnBoot(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Extension point to do additional initialization after composition root is initialized
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected virtual async Task OnBooted(CancellationToken cancellationToken)
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