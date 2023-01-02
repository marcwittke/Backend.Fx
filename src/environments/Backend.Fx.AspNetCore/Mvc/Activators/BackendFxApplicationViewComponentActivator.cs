using System;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.AspNetCore.Mvc.Activators;

[PublicAPI]
public class BackendFxApplicationViewComponentActivator : IViewComponentActivator
{
    private readonly ILogger _logger = Log.Create<BackendFxApplicationViewComponentActivator>();
    private readonly IBackendFxApplication _application;

    public BackendFxApplicationViewComponentActivator(IBackendFxApplication application)
    {
        _application = application;
    }
        
    public object Create(ViewComponentContext context)
    {
        Type requestedViewComponentType = context.ViewComponentDescriptor.TypeInfo.AsType();
        return _application.CompositionRoot.ServiceProvider.GetRequiredService(requestedViewComponentType);
    }
        
    public void Release(ViewComponentContext context, object viewComponent)
    {
    }
}