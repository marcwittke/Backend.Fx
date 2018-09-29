using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Logging;

namespace Backend.Fx.Patterns.DependencyInjection
{
    /// <summary>
    /// The root object of the whole backend fx application framework
    /// </summary>
    public abstract class BackendFxApplication : IBackendFxApplication
    {
        private static readonly ILogger Logger = LogManager.Create<BackendFxApplication>();

        /// <summary>
        /// Initializes the application's runtime instance
        /// </summary>
        /// <param name="compositionRoot">The composition root of the dependency injection framework</param>
        /// <param name="scopeManager">The scope manager for the current application</param>
        protected BackendFxApplication(ICompositionRoot compositionRoot, IScopeManager scopeManager)
        {
            CompositionRoot = compositionRoot;
            ScopeManager = scopeManager;
        }

        /// <inheritdoc />
        public IScopeManager ScopeManager { get; }

        /// <inheritdoc />
        public ICompositionRoot CompositionRoot { get; }

        /// <inheritdoc />
        public ManualResetEventSlim IsBooted { get; } = new ManualResetEventSlim(false);

        /// <inheritdoc />
        public virtual async Task Boot()
        {
            Logger.Info("Booting application");
            CompositionRoot.Verify();
            await Task.CompletedTask;
        }

        protected void BootFinished()
        {
            IsBooted.Set();
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