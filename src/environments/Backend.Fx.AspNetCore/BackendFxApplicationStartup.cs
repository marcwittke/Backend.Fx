using Backend.Fx.AspNetCore.MultiTenancy;
using Backend.Fx.AspNetCore.Mvc;
using Backend.Fx.AspNetCore.Mvc.Activators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Backend.Fx.AspNetCore
{
    public static class BackendFxApplicationStartup
    {
        public static void AddBackendFxApplication<THostedService>(this IServiceCollection services)
            where THostedService : class, IBackendFxApplicationHostedService
        {
            services.AddSingleton<THostedService>();
            services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<THostedService>());
            services.AddSingleton<IControllerActivator, BackendFxApplicationControllerActivator>();
            services.AddSingleton<IViewComponentActivator, BackendFxApplicationViewComponentActivator>();
        }

        public static void UseBackendFxApplication<THostedService, TTenantMiddleware>(this IApplicationBuilder app)
            where THostedService : class, IBackendFxApplicationHostedService
        {
            app.UseMiddleware<TTenantMiddleware>();

            app.Use(
                async (context, requestDelegate) =>
                {
                    // the ambient tenant id has been set before by a TenantMiddleware
                    var tenantId = context.GetTenantId();

                    // the invoking identity has been set before by an AuthenticationMiddleware
                    var actingIdentity = context.User.Identity;

                    var application = app.ApplicationServices.GetRequiredService<THostedService>().Application;

                    await application.AsyncInvoker.InvokeAsync(
                        ip =>
                        {
                            // set the instance provider for activators being called inside the requestDelegate (everything related to MVC
                            // for example, like ControllerActivator and ViewComponentActivator etc.)
                            context.SetCurrentInstanceProvider(ip);

                            return requestDelegate.Invoke();
                        },
                        actingIdentity,
                        tenantId);
                });
        }
    }
}
