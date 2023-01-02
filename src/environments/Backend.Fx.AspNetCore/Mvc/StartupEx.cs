using Backend.Fx.ExecutionPipeline;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.AspNetCore.Mvc;

[PublicAPI]
public static class StartupEx
{
    public static void AddBackendFxMvcApplication(this IServiceCollection services, IBackendFxApplication application)
    {
        application.EnableFeature(new AspNetMvcFeature(services));
    }

    public static void UseBackendFxMvcApplication(this IApplicationBuilder app, IBackendFxApplication application)
    {
        app.Use(async (context, requestDelegate) =>
        {
            // make sure it finished the boot process
            await application.WaitForBootAsync().ConfigureAwait(false);

            await application.Invoker.InvokeAsync(
                (_, _) => requestDelegate.Invoke(),
                context.User.Identity ?? new AnonymousIdentity(),
                context.RequestAborted);
        });
    }
}