using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.AspNetCore.Scoping;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Backend.Fx.AspNetCore
{
    public static class BackendFxApplicationIntegration
    {
        private static readonly ILogger Logger = LogManager.Create(typeof(BackendFxApplicationIntegration));

        public static void AddBackendFxApplication(this IServiceCollection services, IBackendFxApplication application)
        {
            services.AddSingleton(application);
        }

        public static void UseBackendFxApplication<TBackendFxMiddleware>(this IApplicationBuilder app)
            where TBackendFxMiddleware : IBackendFxMiddleware
        {
            app.UseMiddleware<TBackendFxMiddleware>();

            // booting the application when web host enters started phase
            app.ApplicationServices
               .GetRequiredService<IHostApplicationLifetime>()
               .ApplicationStarted
               .Register(async () =>
               {
                   try
                   {
                       var application = app.ApplicationServices.GetService<IBackendFxApplication>();

                       if (SynchronizationContext.Current == null)
                       {
                           // normal ASP.Net Core environment does not have a synchronization context, 
                           // no problem with await here, it will be executed on the thread pool
                           await application.Boot();
                       }
                       else
                       {
                           // xunit uses it's own SynchronizationContext that allows a maximum thread count
                           // equal to the logical cpu count (that is 1 on our single cpu build agents). So
                           // when we're trying to await something here, the task gets scheduled to xunit  
                           // synchronization context, which is already at it's limit running the test thread
                           // so we end up in a deadlock here.
                           // solution is to run the await explicitly on the thread pool by using Task.Run
                           Task.Run(() => application.Boot()).Wait();
                       }

                       Logger.Info("Application startup finished successfully");
                   }
                   catch (Exception ex)
                   {
                       Logger.Fatal(ex, "Application could not be started");
                       app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>().StopApplication();
                   }
               });

            // backendFxApplication should be gracefully disposed on shutdown
            app.ApplicationServices
               .GetRequiredService<IHostApplicationLifetime>()
               .ApplicationStopping
               .Register(app.ApplicationServices.GetService<IBackendFxApplication>().Dispose);
        }
    }
}