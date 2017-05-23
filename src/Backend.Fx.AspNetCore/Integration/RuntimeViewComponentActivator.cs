namespace Backend.Fx.AspNetCore.Integration
{
    using Microsoft.AspNetCore.Mvc.ViewComponents;
    using Patterns.DependencyInjection;

    public class RuntimeViewComponentActivator : RuntimeActivator, IViewComponentActivator
    {
        public RuntimeViewComponentActivator(IRuntime runtime) : base(runtime)
        {}

        public object Create(ViewComponentContext context)
        {
            return GetInstance(context.ViewComponentDescriptor.TypeInfo.AsType());
        }

        public void Release(ViewComponentContext context, object viewComponent)
        {}
    }
}