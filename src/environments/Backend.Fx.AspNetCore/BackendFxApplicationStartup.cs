using System.Security.Principal;
using Backend.Fx.AspNetCore.MultiTenancy;
using Backend.Fx.AspNetCore.Mvc;
using Backend.Fx.AspNetCore.Mvc.Activators;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
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
            services.AddSingleton(provider => provider.GetRequiredService<THostedService>().Application);
            services.AddSingleton<IControllerActivator, BackendFxApplicationControllerActivator>();
        }

        public static void UseBackendFxApplication<THostedService, TTenantMiddleware>(this IApplicationBuilder app)
            where THostedService : class, IBackendFxApplicationHostedService
        {
            app.UseMiddleware<TTenantMiddleware>();

            app.Use(async (context, requestDelegate) =>
            {
                IBackendFxApplication application = app.ApplicationServices.GetRequiredService<THostedService>().Application;
                application.WaitForBoot();

                // set the instance provider for the controller activator
                context.SetCurrentInstanceProvider(application.CompositionRoot.InstanceProvider);

                // the ambient tenant id has been set before by a TenantMiddleware
                var tenantId = context.GetTenantId();

                // the invoking identity has been set before by an AuthenticationMiddleware
                IIdentity actingIdentity = context.User.Identity;

                await application.AsyncInvoker.InvokeAsync(_ => requestDelegate.Invoke(), actingIdentity, tenantId);
            });
        }
    }
}