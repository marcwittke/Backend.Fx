namespace Backend.Fx.AspNetCore.Integration
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Patterns.DependencyInjection;

    public class RuntimeControllerActivator : RuntimeActivator, IControllerActivator
    {
        public RuntimeControllerActivator(IRuntime runtime) : base(runtime)
        {}

        public virtual object Create(ControllerContext c)
        {
            return GetInstance(c.ActionDescriptor.ControllerTypeInfo.AsType());
        }

        public virtual void Release(ControllerContext c, object controller)
        { }
    }
}
