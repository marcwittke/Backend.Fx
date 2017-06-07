namespace DemoBlog.Mvc
{
    using Backend.Fx.AspNetCore.Integration;
    using Backend.Fx.AspNetCore.Middlewares;
    using Backend.Fx.Environment.MultiTenancy;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Backend.Fx.Patterns.DependencyInjection;
    using Bootstrapping;
    using Controllers;
    using Data.Identity;
    using Microsoft.ApplicationInsights.AspNetCore;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.ViewComponents;
    using Microsoft.EntityFrameworkCore;
    using Persistence;
    using Services;
    using SimpleInjector;

    public class Startup
    {
        protected IConfigurationRoot Configuration;
        private readonly DemoBlogRuntime runtime;
        private readonly RuntimeControllerActivator controllerActivator;
        private readonly RuntimeViewComponentActivator viewComponentActivator;
        private readonly ICurrentTHolder<TenantId> defaultTenantIdHolder = new CurrentTenantIdHolder();

        public Startup(IHostingEnvironment env)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            DbContextOptions dbContextOptions = InitializeDatabase(Configuration);

            runtime = new DemoBlogRuntime(env.IsDevelopment(), dbContextOptions);
            controllerActivator = new RuntimeControllerActivator(runtime);
            viewComponentActivator = new RuntimeViewComponentActivator(runtime);
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // tell the framework container to use our runtime to resolve mvc stuff
            services.AddSingleton<IControllerActivator>(controllerActivator);
            services.AddSingleton<IViewComponentActivator>(viewComponentActivator);

            // put the singleton runtime into the framework controller, so that the scope middleware can be resolved
            services.AddSingleton<IScopeManager>(runtime);

            // this is required for the NukeMiddleware
            services.AddSingleton(runtime.DatabaseManager);

            AddIdentityAsFrameworkService(services, Configuration.GetConnectionString("BlogDbConnection"));

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

            app.ApplicationServices.GetRequiredService<IApplicationLifetime>().ApplicationStopping.Register(() => runtime.Dispose());

            // controllers that use ASP.NET Identity are not resolved using the application container, but the framework container
            controllerActivator.RegisterFrameworkOnlyService(() => app.ApplicationServices.GetService<AccountController>());
            controllerActivator.RegisterFrameworkOnlyService(() => app.ApplicationServices.GetService<ManageController>());

            app.UseMiddleware<NukeMiddleware>();
            app.UseMiddleware<VersionHeaderMiddleware>();
            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715
            app.UseGoogleAuthentication(new GoogleOptions
            {
                AuthenticationScheme = "Google",
                DisplayName = "Google",
                SignInScheme = "Identity.External",

                // values stolen from Domick Baier, who encourages use of these credentials with localhost:5000 here: 
                // http://docs.identityserver.io/en/release/quickstarts/4_external_authentication.html#adding-google-support
                ClientId = "434483408261-55tc8n0cs4ff1fe21ea8df2o443v2iuc.apps.googleusercontent.com",
                ClientSecret = "3gcoTrEDPPJ0ukn_aYYT6PWo"
            });

            app.UseMiddleware<DemoBlogMiddleware>();
            
            runtime.Boot(container =>
            {
                container.RegisterMvcViewComponents(app);
                container.Register(() => app.ApplicationServices.GetService<JavaScriptSnippet>());
            });

            defaultTenantIdHolder.ReplaceCurrent(runtime.DefaultTenantId);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private static DbContextOptions InitializeDatabase(IConfigurationRoot configuration)
        {
            var dbContextOptions = new DbContextOptionsBuilder()
                .UseSqlServer(configuration.GetConnectionString("BlogDbConnection"), bld => bld.MigrationsAssembly("DemoBlog.Mvc"))
                .Options;

            using (var blogDbContext = new BlogDbContext(dbContextOptions))
            {
                blogDbContext.Database.Migrate();
            }

            using (var blogIdentityDbContext = new BlogIdentityDbContext(dbContextOptions))
            {
                blogIdentityDbContext.Database.Migrate();
            }
            return dbContextOptions;
        }

        private void AddIdentityAsFrameworkService(IServiceCollection services, string connectionString)
        {
            services.AddDbContext<BlogIdentityDbContext>(options => options.UseSqlServer(connectionString));
            services.AddIdentity<BlogUser, IdentityRole>()
                .AddEntityFrameworkStores<BlogIdentityDbContext>()
                .AddDefaultTokenProviders();
            services.AddScoped<AccountController>();
            services.AddScoped<ManageController>();
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddSingleton(defaultTenantIdHolder);
        }
    }
}
