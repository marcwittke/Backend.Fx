namespace Backend.Fx.Bootstrapping
{
    using System;
    using Environment.MultiTenancy;
    using Environment.Persistence;
    using Logging;
    using Patterns.DependencyInjection;

    /// <summary>
    /// The root object of the whole backend fx application framework
    /// </summary>
    public class BackendFxApplication : IDisposable
    {
        private static readonly ILogger Logger = LogManager.Create<BackendFxApplication>();

        /// <summary>
        /// Initializes the application's runtime instance
        /// </summary>
        /// <param name="compositionRoot">The composition root of the dependency injection framework</param>
        /// <param name="databaseManager">The database manager for the current application</param>
        /// <param name="tenantManager">The tenant manager for the current application</param>
        /// <param name="scopeManager">The scope manager for the current application</param>
        public BackendFxApplication(
                          ICompositionRoot compositionRoot,
                          IDatabaseManager databaseManager,
                          ITenantManager tenantManager,
                          IScopeManager scopeManager)
        {
            CompositionRoot = compositionRoot;
            DatabaseManager = databaseManager;
            TenantManager = tenantManager;
            ScopeManager = scopeManager;
        }

        /// <summary>
        /// The utility instance for database management
        /// </summary>
        public IDatabaseManager DatabaseManager { get; }

        /// <summary>
        /// Access and maintains application tenants
        /// </summary>
        public ITenantManager TenantManager { get; }

        /// <summary>
        /// You should use the <see cref="IScopeManager"/> to open an injection scope for every logical operation.
        /// In case of web applications, this refers to a single HTTP request, for example.
        /// </summary>
        public IScopeManager ScopeManager { get; }

        public ICompositionRoot CompositionRoot { get; }

        public void Boot()
        {
            Logger.Info("Booting application");
            CompositionRoot.Verify();
            DatabaseManager.EnsureDatabaseExistence();
        }

        public void Dispose()
        {
            Logger.Info("Application shut down initialized");
            CompositionRoot?.Dispose();
        }
    }
}