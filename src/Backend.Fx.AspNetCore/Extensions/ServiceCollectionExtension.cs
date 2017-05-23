//namespace Backend.Fx.Web.Extensions
//{
//    using Logging;
//    using Microsoft.AspNetCore.Builder;
//    using Microsoft.AspNetCore.Hosting;
//    using Microsoft.Extensions.DependencyInjection;
//    using Middlewares;
//    using Patterns.DependencyInjection;

//    public static class ServiceCollectionExtension
//    {
//        private static readonly ILogger Logger = LogManager.Create(typeof(ServiceCollectionExtension));

//        /// <summary>
//        ///     Puts the application runtime as singleton into the framework container, including some dependent services
//        /// </summary>
//        public static IServiceCollection AddRuntime(this IServiceCollection services, IRuntime runtime)
//        {
//            services.AddSingleton<IScopeManager>(runtime);
//            runtime.Boot();
//            return services;
//        }

//        /// <summary>
//        ///     Boots the application runtime and configures the HTTP pipeline to use the scope middleware for all requests
//        /// </summary>
//        public static IApplicationBuilder UseScopeMiddleware(this IApplicationBuilder app)
//        {
//            var runtime = app.ApplicationServices.GetRequiredService<IRuntime>();

//            // booting application runtime and registering application stopping event as runtime disposal command
//            app.ApplicationServices.GetRequiredService<IApplicationLifetime>().ApplicationStopping.Register(() => runtime.Dispose());
            
//            // runtime is up and running, now it's time to add the scope middleware to the HTTP pipeline as middleware
//            return app.UseMiddleware<ScopeMiddleware>();
//        }
//    }
//}
