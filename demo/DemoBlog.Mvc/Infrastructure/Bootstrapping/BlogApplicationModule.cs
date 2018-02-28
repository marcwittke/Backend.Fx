namespace DemoBlog.Mvc.Infrastructure.Bootstrapping
{
    using System.Reflection;
    using Backend.Fx.Bootstrapping.Modules;
    using Backend.Fx.Environment.DateAndTime;
    using Domain;
    using SimpleInjector;

    public class BlogApplicationModule : ApplicationModule
    {
        public BlogApplicationModule() : base(typeof(Blog).GetTypeInfo().Assembly)
        { }

        protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
        {
            base.Register(container, scopedLifestyle);
            container.Register<IClock, FrozenClock>();
        }
    }
}