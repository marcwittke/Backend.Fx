using Backend.Fx.AspNetCore.Mvc.Activators;
using Backend.Fx.Features;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.AspNetCore.Mvc;

[PublicAPI]
public class AspNetMvcFeature : Feature
{
    private readonly IServiceCollection _frameworkServices;

    public AspNetMvcFeature(IServiceCollection frameworkServices)
    {
        _frameworkServices = frameworkServices;
    }

    public override void Enable(IBackendFxApplication application)
    {
        // tell ASP.Net Core to use BackendFx to create controller instances 
        _frameworkServices.AddSingleton<IControllerActivator>(
            new BackendFxApplicationControllerActivator(application));

        // tell ASP.Net Core to use BackendFx to create view component instances
        _frameworkServices.AddSingleton<IViewComponentActivator>(
            new BackendFxApplicationViewComponentActivator(application));

        application.CompositionRoot.RegisterModules(new AspNetMvcModule(application.Assemblies));
    }
}