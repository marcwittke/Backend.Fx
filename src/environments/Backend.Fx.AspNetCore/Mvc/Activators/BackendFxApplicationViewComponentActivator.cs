﻿using System;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace Backend.Fx.AspNetCore.Mvc.Activators
{
    public class BackendFxApplicationViewComponentActivator : IViewComponentActivator
    {
        private static readonly ILogger Logger = LogManager.Create<BackendFxApplicationViewComponentActivator>();

        public object Create(ViewComponentContext context)
        {
            var requestedViewComponentType = context.ViewComponentDescriptor.TypeInfo.AsType();

            return context.ViewContext.HttpContext.TryGetInstanceProvider(out var ip)
                ? CreateInstanceUsingInstanceProvider(ip, requestedViewComponentType)
                : CreateInstanceUsingSystemActivator(requestedViewComponentType);
        }

        public void Release(ViewComponentContext context, object viewComponent)
        { }

        private static object CreateInstanceUsingInstanceProvider(object ip, Type requestedViewComponentType)
        {
            Logger.Debug($"Providing {requestedViewComponentType.Name} using {ip.GetType().Name}");
            return ((IInstanceProvider)ip).GetInstance(requestedViewComponentType);
        }

        private static object CreateInstanceUsingSystemActivator(Type requestedViewComponentType)
        {
            Logger.Debug($"Providing {requestedViewComponentType.Name} using {nameof(Activator)}");
            return Activator.CreateInstance(requestedViewComponentType);
        }
    }
}
