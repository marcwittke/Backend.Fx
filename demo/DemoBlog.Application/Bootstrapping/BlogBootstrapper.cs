namespace DemoBlog.Bootstrapping
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Backend.Fx.Bootstrapping;
    using Backend.Fx.Bootstrapping.Modules;
    using Backend.Fx.EfCorePersistence;
    using Backend.Fx.Environment.MultiTenancy;
    using Domain;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Persistence;

    public class BlogBootstrapper
    {
        public BackendFxApplication BuildApplication(string connectionString, Action<SqlServerDbContextOptionsBuilder> optionAction)
        {
            DbContextOptions<BlogDbContext> blogDbContextOptions = new DbContextOptionsBuilder<BlogDbContext>()
                    .UseSqlServer(connectionString, optionAction)
                    .Options;

            // application composition root initialization
            SimpleInjectorCompositionRoot compositionRoot = new SimpleInjectorCompositionRoot();
            compositionRoot.RegisterModules(
                new DomainModule(compositionRoot, typeof(Blog).GetTypeInfo().Assembly),
                new EfCorePersistenceModule<BlogDbContext>(compositionRoot, blogDbContextOptions),
                new BlogModule(compositionRoot));

            var backendFxApplication = new BackendFxApplication(
                compositionRoot,
                new DatabaseManagerWithMigration<BlogDbContext>(blogDbContextOptions),
                new TenantManager<BlogDbContext>(new TenantInitializer(compositionRoot), blogDbContextOptions),
                compositionRoot);

            return backendFxApplication;
        }

        /// <summary>
        /// This is only supported in development environments. Running inside of an IIS host will result in timeouts during the first 
        /// request, leaving the system in an unpredicted state. To achieve the same effect in a hosted demo environment, use the same
        /// functionality via service endpoints.
        /// </summary>
        public void EnsureDevelopmentTenantExistence(BackendFxApplication backendFxApplication)
        {
            if (backendFxApplication.TenantManager.GetTenants().Any(t => t.IsDemoTenant && t.Name == "dev"))
            {
                return;
            }

            // This will create a demonstration tenant. Note that using the TenantManager directly there won't be any TenantCreated event published...
            TenantId tenantId = backendFxApplication.TenantManager.CreateDemonstrationTenant("dev", "dev tenant", true, new CultureInfo("en-US"));

            // ... therefore it's up to us to do the initialization. Which is fine, because we are not spinning of a background action but blocking in our thread.
            backendFxApplication.TenantManager.EnsureTenantIsInitialized(tenantId);
        }
    }
}
