namespace DemoBlog.Mvc.Infrastructure.Bootstrapping
{
    using Backend.Fx.EfCorePersistence;
    using Backend.Fx.Environment.Persistence;
    using Controllers;
    using Data.Identity;
    using Integration;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Services;
    using SimpleInjector;

    public class IdentityApplication
    {
        private readonly string connectionString;

        public IdentityApplication(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // initialize identity as framework service
            services.AddDbContext<BlogIdentityDbContext>(optionsBuilder => optionsBuilder.UseSqlServer(connectionString));
            services.AddIdentity<BlogUser, IdentityRole>()
                    .AddEntityFrameworkStores<BlogIdentityDbContext>()
                    .AddDefaultTokenProviders();
            services.ConfigureApplicationCookie(options => options.LoginPath = "/Account/Login");
            services.AddAuthentication()
                    .AddGoogle(options =>
                    {
                        options.ClientId = "434483408261-55tc8n0cs4ff1fe21ea8df2o443v2iuc.apps.googleusercontent.com";
                        options.ClientSecret = "3gcoTrEDPPJ0ukn_aYYT6PWo";
                    });

            services.AddScoped<AccountController>();
            services.AddScoped<ManageController>();
            services.AddTransient<IEmailSender, AuthMessageSender>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // controllers that use ASP.NET Identity are not resolved using the application container, but the framework container
            var compositionRootControllerActivator = (CompositionRootControllerActivator)app.ApplicationServices.GetRequiredService<IControllerActivator>();
            compositionRootControllerActivator.RegisterFrameworkOnlyService(app.GetRequiredRequestService<AccountController>);
            compositionRootControllerActivator.RegisterFrameworkOnlyService(app.GetRequiredRequestService<ManageController>);

            app.UseAuthentication();
        }

        public void Boot()
        {
            // identity database initialization
            DbContextOptions<BlogIdentityDbContext> identityDbContextOptions =
                    new DbContextOptionsBuilder<BlogIdentityDbContext>()
                            .UseSqlServer(connectionString, bld => {
                                                                bld.MigrationsAssembly("DemoBlog.Mvc");
                                                                bld.MigrationsHistoryTable("__EFMigrationsHistory", "identity");
                                                            })
                            .Options;

            IDatabaseManager identityDatabaseManager = new DatabaseManagerWithMigration<BlogIdentityDbContext>(identityDbContextOptions);
            identityDatabaseManager.EnsureDatabaseExistence();
        }
    }
}
