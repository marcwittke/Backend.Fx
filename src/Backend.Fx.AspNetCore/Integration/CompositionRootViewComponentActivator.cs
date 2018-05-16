namespace Backend.Fx.AspNetCore.Integration
{
    using Microsoft.AspNetCore.Mvc.ViewComponents;
    using Patterns.DependencyInjection;

    public class CompositionRootViewComponentActivator : CompositionRootActivator, IViewComponentActivator
    {
        public CompositionRootViewComponentActivator(ICompositionRoot compositionRoot) : base(compositionRoot)
        { }

        public object Create(ViewComponentContext context)
        {
            return GetInstance(context.ViewComponentDescriptor.TypeInfo.AsType());
        }

        public void Release(ViewComponentContext context, object viewComponent)
        { }
    }
}