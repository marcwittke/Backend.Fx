namespace Backend.Fx.AspNetCore
{
    using Logging;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Patterns.DependencyInjection;

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