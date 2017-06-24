namespace DemoBlog.Mvc.Infrastructure
{
    using Backend.Fx.Patterns.DependencyInjection;
    using Microsoft.AspNetCore.Mvc.ViewComponents;

    public class RuntimeViewComponentActivator : RuntimeActivator, IViewComponentActivator
    {
        public RuntimeViewComponentActivator(IRuntime runtime) : base(runtime)
        { }

        public object Create(ViewComponentContext context)
        {
            return GetInstance(context.ViewComponentDescriptor.TypeInfo.AsType());
        }

        public void Release(ViewComponentContext context, object viewComponent)
        { }
    }
}