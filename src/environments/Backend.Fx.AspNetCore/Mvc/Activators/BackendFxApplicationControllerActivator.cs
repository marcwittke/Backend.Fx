using System;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.AspNetCore.Mvc.Activators;

[PublicAPI]
public class BackendFxApplicationControllerActivator : IControllerActivator
{
    private readonly IBackendFxApplication _application;
    private static readonly ILogger Logger = Log.Create<BackendFxApplicationControllerActivator>();

    public BackendFxApplicationControllerActivator(IBackendFxApplication application)
    {
        _application = application;
    }
        
    public virtual object Create(ControllerContext c)
    {
        Type requestedControllerType = c.ActionDescriptor.ControllerTypeInfo.AsType();
        return _application.CompositionRoot.ServiceProvider.GetRequiredService(requestedControllerType);
    }

    public virtual void Release(ControllerContext c, object controller)
    {
        Logger.LogTrace("Releasing {ControllerTypeName}", controller.GetType().Name);
        if (controller is IDisposable disposable)
        {
            Logger.LogDebug("Disposing {ControllerTypeName}", controller.GetType().Name);
            disposable.Dispose();
        }
    }
}