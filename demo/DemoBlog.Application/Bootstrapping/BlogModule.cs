namespace DemoBlog.Bootstrapping
{
    using Backend.Fx.Bootstrapping;
    using Backend.Fx.Bootstrapping.Modules;
    using Backend.Fx.Patterns.IdGeneration;
    using Persistence;
    using SimpleInjector;

    public class BlogModule : SimpleInjectorModule
    {
        public BlogModule(SimpleInjectorCompositionRoot compositionRoot) : base(compositionRoot)
        { }

        protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
        {
            container.RegisterSingleton<IEntityIdGenerator, BlogEntityIdGenerator>();
        }
    }
}