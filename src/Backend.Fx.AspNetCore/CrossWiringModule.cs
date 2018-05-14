namespace Backend.Fx.AspNetCore
{
    using Bootstrapping.Modules;
    using Microsoft.Extensions.DependencyInjection;
    using SimpleInjector;

    public class CrossWiringModule : SimpleInjectorModule
    {
        private readonly IServiceCollection services;

        public CrossWiringModule(IServiceCollection services)
        {
            this.services = services;
        }

        protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
        {
            services.UseSimpleInjectorAspNetRequestScoping(container);
            services.EnableSimpleInjectorCrossWiring(container);
        }
    }
}
