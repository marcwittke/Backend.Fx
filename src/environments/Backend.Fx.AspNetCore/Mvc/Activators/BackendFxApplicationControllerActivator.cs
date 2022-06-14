using System;
using Backend.Fx.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.AspNetCore.Mvc.Activators
{
    /// <summary>
    /// This controller activator relies on an <see cref="IServiceProvider"/> set before in the
    /// http context items dictionary. If non is to be found, the controller is activated
    /// using the default <see cref="System.Activator"/> (without providing any ctor arguments).
    /// </summary>
    public class BackendFxApplicationControllerActivator : IControllerActivator
    {
        private static readonly ILogger Logger = Log.Create<BackendFxApplicationControllerActivator>();

        public virtual object Create(ControllerContext c)
        {
            var requestedControllerType = c.ActionDescriptor.ControllerTypeInfo.AsType();
            
            return c.HttpContext.TryGetServiceProvider(out var ip) 
                ? CreateInstanceUsingServiceProvider(ip, requestedControllerType)
                : CreateInstanceUsingSystemActivator(requestedControllerType);
        }

        private static object CreateInstanceUsingServiceProvider(object sp, Type requestedControllerType)
        {
            Logger.LogDebug("Providing {ControllerTypeName} using {ServiceProvider}", requestedControllerType.Name, sp.GetType().Name);
            return ((IServiceProvider)sp).GetRequiredService(requestedControllerType);
        }

        private static object CreateInstanceUsingSystemActivator(Type requestedControllerType)
        {
            Logger.LogDebug("Providing {ControllerTypeName} using {Activator}", requestedControllerType.Name, nameof(Activator));
            return Activator.CreateInstance(requestedControllerType);
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
}