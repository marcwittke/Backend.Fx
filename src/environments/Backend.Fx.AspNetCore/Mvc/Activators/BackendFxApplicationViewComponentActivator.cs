using System;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.AspNetCore.Mvc.Activators
{
    public class BackendFxApplicationViewComponentActivator : IViewComponentActivator
    {
        private static readonly ILogger Logger = Log.Create<BackendFxApplicationViewComponentActivator>();
        
        public object Create(ViewComponentContext context)
        {
            var requestedViewComponentType = context.ViewComponentDescriptor.TypeInfo.AsType();
            
            return context.ViewContext.HttpContext.TryGetInstanceProvider(out var ip) 
                ? CreateInstanceUsingInstanceProvider(ip, requestedViewComponentType)
                : CreateInstanceUsingSystemActivator(requestedViewComponentType);
        }

        private static object CreateInstanceUsingInstanceProvider(object ip, Type requestedViewComponentType)
        {
            Logger.LogDebug("Providing {ViewComponentName} using {InstanceProvider}", requestedViewComponentType.Name, ip.GetType().Name);
            return ((IInstanceProvider)ip).GetInstance(requestedViewComponentType);
        }

        private static object CreateInstanceUsingSystemActivator(Type requestedViewComponentType)
        {
            Logger.LogDebug("Providing {ViewComponentName} using {InstanceProvider}", requestedViewComponentType.Name, nameof(Activator));
            return Activator.CreateInstance(requestedViewComponentType);
        }
        
        public void Release(ViewComponentContext context, object viewComponent)
        {
        }
    }
}