namespace DemoBlog.Mvc
{
    using Backend.Fx.Bootstrapping;
    using Backend.Fx.EfCorePersistence;
    using Backend.Fx.Environment.Persistence;
    using Bootstrapping;
    using Controllers;
    using Data.Identity;
    using Infrastructure;
    using Microsoft.AspNetCore.Authentication.Google;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.ViewComponents;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Services;

    public class Startup
    {
        private readonly BlogBootstrapper blogBootstrapper = new BlogBootstrapper();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("BlogDbConnection");
            
            // identity database initialization
            DbContextOptions<BlogIdentityDbContext> identityDbContextOptions = new DbContextOptionsBuilder<BlogIdentityDbContext>()
                    .UseSqlServer(connectionString, bld => bld.MigrationsAssembly("DemoBlog.Mvc"))
                    .Options;
            IDatabaseManager identityDatabaseManager = new DatabaseManagerWithMigration<BlogIdentityDbContext>(identityDbContextOptions);
            identityDatabaseManager.EnsureDatabaseExistence();

            // initialize identity as framework service
            services.AddDbContext<BlogIdentityDbContext>(optionsBuilder => optionsBuilder.UseSqlServer(connectionString));
            services.AddIdentity<BlogUser, IdentityRole>()
                    .AddEntityFrameworkStores<BlogIdentityDbContext>()
                    .AddDefaultTokenProviders();
            services.ConfigureApplicationCookie(options => options.LoginPath = "/Account/Login");
            services.AddAuthentication()
                    .AddGoogle(options => {
                                   options.ClientId = "434483408261-55tc8n0cs4ff1fe21ea8df2o443v2iuc.apps.googleusercontent.com";
                                   options.ClientSecret = "3gcoTrEDPPJ0ukn_aYYT6PWo";
                               });

            services.AddScoped<AccountController>();
            services.AddScoped<ManageController>();
            services.AddTransient<IEmailSender, AuthMessageSender>();

            var backendFxApplication = blogBootstrapper.BuildApplication(connectionString, options => options.MigrationsAssembly("DemoBlog.Mvc"));
            backendFxApplication.Boot();
            blogBootstrapper.EnsureDevelopmentTenantExistence(backendFxApplication);

            // tell the framework container to use our composition root to resolve mvc stuff
            services.AddSingleton<IControllerActivator>(new CompositionRootControllerActivator(backendFxApplication.CompositionRoot));
            services.AddSingleton<IViewComponentActivator>(new CompositionRootViewComponentActivator(backendFxApplication.CompositionRoot));

            // put the singleton application into the framework controller, so that the application middleware can be resolved
            services.AddSingleton(backendFxApplication);
            
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            // backendFxApplication should be gracefully disposed on shutdown
            app.ApplicationServices.GetRequiredService<IApplicationLifetime>().ApplicationStopping.Register(() => app.ApplicationServices.GetRequiredService<BackendFxApplication>().Dispose());

            // controllers that use ASP.NET Identity are not resolved using the application container, but the framework container
            var compositionRootControllerActivator = (CompositionRootControllerActivator)app.ApplicationServices.GetRequiredService<IControllerActivator>();
            compositionRootControllerActivator.RegisterFrameworkOnlyService(() => app.ApplicationServices.GetService<AccountController>());
            compositionRootControllerActivator.RegisterFrameworkOnlyService(() => app.ApplicationServices.GetService<ManageController>());

            app.UseMiddleware<BackendFxMiddleware>();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
