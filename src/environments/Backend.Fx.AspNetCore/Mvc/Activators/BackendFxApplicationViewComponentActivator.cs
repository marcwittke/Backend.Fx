using System;
using Backend.Fx.Logging;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.DependencyInjection;
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
            
            return context.ViewContext.HttpContext.TryGetServiceProvider(out var sp) 
                ? CreateInstanceUsingServiceProvider(sp, requestedViewComponentType)
                : CreateInstanceUsingSystemActivator(requestedViewComponentType);
        }

        private static object CreateInstanceUsingServiceProvider(object sp, Type requestedViewComponentType)
        {
            Logger.LogDebug("Providing {ViewComponentName} using {ServiceProvider}", requestedViewComponentType.Name, sp.GetType().Name);
            return ((IServiceProvider)sp).GetRequiredService(requestedViewComponentType);
        }

        private static object CreateInstanceUsingSystemActivator(Type requestedViewComponentType)
        {
            Logger.LogDebug("Providing {ViewComponentName} using {ServiceProvider}", requestedViewComponentType.Name, nameof(Activator));
            return Activator.CreateInstance(requestedViewComponentType);
        }
        
        public void Release(ViewComponentContext context, object viewComponent)
        {
        }
    }
}