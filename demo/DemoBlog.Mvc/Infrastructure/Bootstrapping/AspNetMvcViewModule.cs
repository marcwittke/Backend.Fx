namespace DemoBlog.Mvc.Infrastructure.Bootstrapping
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
            // don't do this, it will result in unresolvable constructor parameters (IUserManager) in AccountController and ManageController
            // SimpleInjector can resolve all controllers even when not registered explicitly
            // container.RegisterMvcControllers(applicationBuilder);
            
            container.RegisterMvcViewComponents(applicationBuilder);
            container.Register(() => applicationBuilder.ApplicationServices.GetService<JavaScriptSnippet>());
        }
    }
}