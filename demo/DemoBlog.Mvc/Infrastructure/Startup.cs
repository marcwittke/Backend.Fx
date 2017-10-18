namespace DemoBlog.Mvc.Infrastructure
{
    using Backend.Fx.EfCorePersistence;
    using Backend.Fx.Environment.Persistence;
    using Controllers;
    using Data.Identity;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Services;

    public class Startup
    {
        protected IConfigurationRoot Configuration;

        public Startup(IHostingEnvironment env)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }
        
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
            services.AddScoped<AccountController>();
            services.AddScoped<ManageController>();
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            
            services.AddBackendFx(connectionString);

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

            app.UseBackendFx();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
