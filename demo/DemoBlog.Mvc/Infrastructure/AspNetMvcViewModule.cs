namespace DemoBlog.Mvc.Infrastructure
{
    using Backend.Fx.Bootstrapping;
    using Backend.Fx.Bootstrapping.Modules;
    using Microsoft.ApplicationInsights.AspNetCore;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using SimpleInjector;

    public class AspNetMvcViewModule : SimpleInjectorModule
    {
        private readonly IApplicationBuilder applicationBuilder;

        public AspNetMvcViewModule(SimpleInjectorCompositionRoot compositionRoot, IApplicationBuilder applicationBuilder) : base(compositionRoot)
        {
            this.applicationBuilder = applicationBuilder;
        }

        protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
        {
            container.RegisterMvcViewComponents(applicationBuilder);
            container.Register(() => applicationBuilder.ApplicationServices.GetService<JavaScriptSnippet>());
        }
    }
}