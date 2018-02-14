namespace DemoBlog.Mvc.Infrastructure.Bootstrapping
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class Startup
    {
        private readonly BlogApplication blogApplication;
        private readonly IdentityApplication identityApplication;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            var connectionString = Configuration.GetConnectionString("BlogDbConnection");
            NLog.LogManager.Configuration.Variables["connectionString"] = connectionString;
            blogApplication = BlogApplication.Build(connectionString);
            identityApplication = new IdentityApplication(connectionString);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            identityApplication.ConfigureServices(services);
            blogApplication.ConfigureServices(services);
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
        {
            blogApplication.WaitForDatabase(40, 15);

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

            app.UseDefaultFiles();
            app.UseStaticFiles();
            
            // booting identityApplication
            identityApplication.Configure(app, env);
            identityApplication.Boot();

            // booting blogApplication
            blogApplication.Configure(app, env);
            blogApplication.Boot();

            app.UseMvc(routes => routes.MapRoute(name: "default", template: "{controller=Home}/{action=Index}/{id?}"));
        }
    }
}
