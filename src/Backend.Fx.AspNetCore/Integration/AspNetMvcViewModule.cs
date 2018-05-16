namespace Backend.Fx.AspNetCore
{
    using Bootstrapping.Modules;
    using Microsoft.AspNetCore.Builder;
    using SimpleInjector;

    public class AspNetMvcViewModule : SimpleInjectorModule
    {
        private readonly IApplicationBuilder applicationBuilder;

        public AspNetMvcViewModule(IApplicationBuilder applicationBuilder)
        {
            this.applicationBuilder = applicationBuilder;
        }

        protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
        {
            //MVC
            container.RegisterMvcControllers(applicationBuilder);
            container.RegisterMvcViewComponents(applicationBuilder);
        }
    }
}
