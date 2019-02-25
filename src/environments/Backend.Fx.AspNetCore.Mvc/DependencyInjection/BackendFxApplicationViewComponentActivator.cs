using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace Backend.Fx.AspNetCore.Mvc.DependencyInjection
{
    public class BackendFxApplicationViewComponentActivator : BackendFxApplicationActivator, IViewComponentActivator
    {
        public BackendFxApplicationViewComponentActivator(IBackendFxApplication application) : base(application)
        { }

        public object Create(ViewComponentContext context)
        {
            return GetInstance(context.ViewComponentDescriptor.TypeInfo.AsType());
        }

        public void Release(ViewComponentContext context, object viewComponent)
        { }
    }
}