namespace DemoBlog.Mvc.Infrastructure
{
    using System.Reflection;
    using Backend.Fx.Bootstrapping;
    using Backend.Fx.Bootstrapping.Modules;
    using Backend.Fx.EfCorePersistence;
    using Backend.Fx.Environment.DateAndTime;
    using Backend.Fx.Environment.MultiTenancy;
    using Backend.Fx.Patterns.DependencyInjection;
    using Controllers;
    using Domain;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.ViewComponents;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Persistence;

    public static class BackendFxMiddlewareStartup
    {
        public static void AddBackendFx(this IServiceCollection services, string connectionString)
        {
            DbContextOptions<BlogDbContext> blogDbContextOptions = new DbContextOptionsBuilder<BlogDbContext>()
                    .UseSqlServer(connectionString, bld => bld.MigrationsAssembly("DemoBlog.Mvc"))
                    .Options;

            // application composition root initialization
            SimpleInjectorCompositionRoot compositionRoot = new SimpleInjectorCompositionRoot();
            compositionRoot.RegisterModules(
                new DomainModule(compositionRoot, typeof(Blog).GetTypeInfo().Assembly),
                new ClockModule<FrozenClock>(compositionRoot),
                new EfCorePersistenceModule<BlogDbContext>(compositionRoot, blogDbContextOptions),
                new DemoBlogModule(compositionRoot));

            // application start
            var backendFxApplication = new BackendFxApplication(
                compositionRoot,
                new DatabaseManagerWithMigration<BlogDbContext>(blogDbContextOptions),
                new TenantManager<BlogDbContext>(new TenantInitializer(compositionRoot), blogDbContextOptions),
                compositionRoot);
            backendFxApplication.Boot();

            // tell the framework container to use our runtime to resolve mvc stuff
            services.AddSingleton<IControllerActivator>(new CompositionRootControllerActivator(compositionRoot));
            services.AddSingleton<IViewComponentActivator>(new CompositionRootViewComponentActivator(compositionRoot));

            // put the singleton application into the framework controller, so that the application middleware can be resolved
            services.AddSingleton(backendFxApplication);


            services.AddScoped(_ => backendFxApplication.CompositionRoot.GetInstance<ICurrentTHolder<TenantId>>());
        }

        public static void UseBackendFx(this IApplicationBuilder app)
        {
            BackendFxApplication backendFxApplication = app.ApplicationServices.GetRequiredService<BackendFxApplication>();

            // backendFxApplication should be gracefully disposed on shutdown
            app.ApplicationServices.GetRequiredService<IApplicationLifetime>().ApplicationStopping.Register(() => backendFxApplication.Dispose());

            // controllers that use ASP.NET Identity are not resolved using the application container, but the framework container
            var compositionRootControllerActivator = (CompositionRootControllerActivator)app.ApplicationServices.GetRequiredService<IControllerActivator>();
            compositionRootControllerActivator.RegisterFrameworkOnlyService(() => app.ApplicationServices.GetService<AccountController>());
            compositionRootControllerActivator.RegisterFrameworkOnlyService(() => app.ApplicationServices.GetService<ManageController>());

            app.UseMiddleware<BackendFxMiddleware>();
        }
    }
}