using System;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Backend.Fx.AspNetCore.Mvc.Activators
{
    /// <summary>
    /// This controller activator relies on an <see cref="IInstanceProvider"/> set before in the
    /// http context items dictionary. If non is to be found, the controller is activated
    /// using the default <see cref="System.Activator"/> (without providing any ctor arguments).
    /// </summary>
    public class BackendFxApplicationControllerActivator : IControllerActivator
    {
        private static readonly ILogger Logger = LogManager.Create<BackendFxApplicationControllerActivator>();

        public virtual object Create(ControllerContext c)
        {
            var requestedControllerType = c.ActionDescriptor.ControllerTypeInfo.AsType();
            
            return c.HttpContext.TryGetInstanceProvider(out var ip) 
                ? CreateInstanceUsingInstanceProvider(ip, requestedControllerType)
                : CreateInstanceUsingSystemActivator(requestedControllerType);
        }

        private static object CreateInstanceUsingInstanceProvider(object ip, Type requestedControllerType)
        {
            Logger.Debug($"Providing {requestedControllerType.Name} using {ip.GetType().Name}");
            return ((IInstanceProvider)ip).GetInstance(requestedControllerType);
        }

        private static object CreateInstanceUsingSystemActivator(Type requestedControllerType)
        {
            Logger.Debug($"Providing {requestedControllerType.Name} using {nameof(Activator)}");
            return Activator.CreateInstance(requestedControllerType);
        }

        public virtual void Release(ControllerContext c, object controller)
        {
            Logger.Trace($"Releasing {controller.GetType().Name}");
            if (controller is IDisposable disposable)
            {
                Logger.Debug($"Disposing {controller.GetType().Name}");
                disposable.Dispose();
            }
        }
    }
}