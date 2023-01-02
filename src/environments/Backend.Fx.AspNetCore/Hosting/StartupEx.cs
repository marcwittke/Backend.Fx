using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Backend.Fx.AspNetCore.Hosting;

[PublicAPI]
public static class StartupEx
{
    public static void AddBackendFxApplication(this IServiceCollection services, IBackendFxApplication application)
    {
        // by wrapping the application in a hosted service and adding it to the service collection we
        // ensure that the application gets booted on Asp.Net Core application start
        IHostedService hostedService = new BackendFxApplicationHostedService(application);
        services.AddSingleton(hostedService);
    }
}