using Microsoft.AspNetCore.Builder;

namespace Backend.Fx.AspNetCore.Integration
{
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
