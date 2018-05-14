namespace Backend.Fx.Bootstrapping
{
    using System;
    using System.Threading.Tasks;
    using Logging;
    using Patterns.DependencyInjection;

    /// <summary>
    /// The root object of the whole backend fx application framework
    /// </summary>
    public abstract class BackendFxApplication : IDisposable
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

        /// <summary>
        /// You should use the <see cref="IScopeManager"/> to open an injection scope for every logical operation.
        /// In case of web applications, this refers to a single HTTP request, for example.
        /// </summary>
        public IScopeManager ScopeManager { get; }

        public ICompositionRoot CompositionRoot { get; }

        public virtual async Task Boot()
        {
            Logger.Info("Booting application");
            CompositionRoot.Verify();
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