using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace Backend.Fx.AspNetCore.Mvc.DependencyInjection
{
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