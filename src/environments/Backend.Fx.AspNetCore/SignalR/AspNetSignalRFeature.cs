using Backend.Fx.Features;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.AspNetCore.SignalR;

[PublicAPI]
public class AspNetSignalRFeature : Feature
{
    private readonly IServiceCollection _frameworkServices;

    public AspNetSignalRFeature(IServiceCollection frameworkServices)
    {
        _frameworkServices = frameworkServices;
    }

    public override void Enable(IBackendFxApplication application)
    {
        application.CompositionRoot.RegisterModules(new AspNetSignalRModule(_frameworkServices, application.Assemblies));
    }
}