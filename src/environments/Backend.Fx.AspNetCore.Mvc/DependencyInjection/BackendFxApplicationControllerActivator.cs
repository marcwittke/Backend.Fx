﻿using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Backend.Fx.AspNetCore.Mvc.DependencyInjection
{
    public class BackendFxApplicationControllerActivator : BackendFxApplicationActivator, IControllerActivator
    {
        private static readonly ILogger Logger = LogManager.Create<BackendFxApplicationControllerActivator>();
        public BackendFxApplicationControllerActivator(IBackendFxApplication application) : base(application)
        { }

        public virtual object Create(ControllerContext c)
        {
            object instance = GetInstance(c.ActionDescriptor.ControllerTypeInfo.AsType());
            if (instance == null)
            {
                Logger.Warn($"Activation of {c.ActionDescriptor.ControllerTypeInfo.AsType()} returned NULL");
            }
            return instance;
        }

        public virtual void Release(ControllerContext c, object controller)
        { }
    }
}