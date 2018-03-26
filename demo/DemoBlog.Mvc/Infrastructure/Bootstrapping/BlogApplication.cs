namespace DemoBlog.Mvc.Infrastructure.Bootstrapping
{
    using System;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Net.Sockets;
    using Backend.Fx.Bootstrapping;
    using Backend.Fx.EfCorePersistence;
    using Backend.Fx.Environment.MultiTenancy;
    using Backend.Fx.Environment.Persistence;
    using Backend.Fx.Logging;
    using Backend.Fx.Patterns.DependencyInjection;
    using Integration;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.ViewComponents;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Middlewares;
    using Persistence;
    using Polly;
    using Polly.Retry;

    public class BlogApplication : BackendFxApplication
    {
        private static readonly ILogger Logger = LogManager.Create<BlogApplication>();
        private readonly string connectionString;
    
        private BlogApplication(string connectionString,
                                ICompositionRoot compositionRoot, 
                                IDatabaseManager databaseManager, 
                                ITenantManager tenantManager,
                                IScopeManager scopeManager) : base(compositionRoot, databaseManager, tenantManager, scopeManager)
        {
            this.connectionString = connectionString;
        }

        public static BlogApplication Build(string connectionString)
        {
            DbContextOptions<BlogDbContext> blogDbContextOptions = new DbContextOptionsBuilder<BlogDbContext>()
                    .UseSqlServer(connectionString, options => options.MigrationsAssembly("DemoBlog.Mvc"))
                    .Options;

            // application composition root initialization
            SimpleInjectorCompositionRoot compositionRoot = new SimpleInjectorCompositionRoot();
            compositionRoot.RegisterModules(
                new BlogPersistenceModule(blogDbContextOptions),
                new BlogDomainModule());

            var blogApplication = new BlogApplication(
                connectionString,
                compositionRoot,
                new DatabaseManagerWithMigration<BlogDbContext>(blogDbContextOptions),
                new TenantManager<BlogDbContext>(new TenantInitializer(compositionRoot), blogDbContextOptions),
                compositionRoot);

            return blogApplication;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // tell the netcore framework container to use our composition root to resolve mvc stuff
            services.AddSingleton<IControllerActivator>(new CompositionRootControllerActivator(CompositionRoot));
            services.AddSingleton<IViewComponentActivator>(new CompositionRootViewComponentActivator(CompositionRoot));

            // the framework need to resolve the current TenantId
            services.AddScoped(_ => CompositionRoot.GetInstance<ICurrentTHolder<TenantId>>());

            // put the singleton application into the netcore framework container, so that it can be injected into the application middleware
            services.AddSingleton(this);
        }

        public void Configure(IApplicationBuilder app)
        {
            CompositionRoot.RegisterModules(new AspNetMvcViewModule(app));

            // this instance should be gracefully disposed on shutdown
            app.ApplicationServices.GetRequiredService<IApplicationLifetime>().ApplicationStopping.Register(Dispose);

            // every request runs through our application middleware, using this instance
            app.UseMiddleware<BlogMiddleware>();
        }

        public void CheckDatabaseExistence()
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            Logger.Info($"Checking existence of database {connectionStringBuilder.DataSource}");

            connectionStringBuilder.InitialCatalog = "tempdb";
            using (var connection = new SqlConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT 'whatever'";
                    command.ExecuteScalar();
                }
            }
        }

        public void WaitForDatabase(int retries, int secondsToWait)
        {
            RetryPolicy retryPolicy = Policy.Handle<Exception>(ex => {
                                                                   if (ex is AggregateException aggEx)
                                                                   {
                                                                       foreach (var aggExInnerException in aggEx.InnerExceptions.Skip(1))
                                                                       {
                                                                           Logger.Info(aggExInnerException);
                                                                       }

                                                                       ex = aggEx.InnerException;
                                                                   }

                                                                   if (ex is SocketException || ex is SqlException)
                                                                   {
                                                                       Logger.Info(ex);
                                                                       return true;
                                                                   }

                                                                   return false;
                                                               })
                                            .WaitAndRetry(retries, attempt => TimeSpan.FromSeconds(secondsToWait));

            retryPolicy.Execute(CheckDatabaseExistence);
        }
    }
}
