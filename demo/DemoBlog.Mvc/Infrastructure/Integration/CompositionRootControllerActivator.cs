﻿namespace DemoBlog.Mvc.Infrastructure.Integration
{
    using Backend.Fx.Logging;
    using Backend.Fx.Patterns.DependencyInjection;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;

    public class CompositionRootControllerActivator : CompositionRootActivator, IControllerActivator
    {
        private static readonly ILogger Logger = LogManager.Create<CompositionRootControllerActivator>();
        public CompositionRootControllerActivator(ICompositionRoot compositionRoot) : base(compositionRoot)
        { }

        public virtual object Create(ControllerContext c)
        {
            var instance = GetInstance(c.ActionDescriptor.ControllerTypeInfo.AsType());
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