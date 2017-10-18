namespace Backend.Fx.Bootstrapping.Modules
{
    using Environment.DateAndTime;
    using SimpleInjector;

    public class ClockModule<TClock> : SimpleInjectorModule where TClock : class, IClock
    {
        public ClockModule(SimpleInjectorCompositionRoot compositionRoot) : base(compositionRoot)
        { }

        protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
        {
            container.Register<IClock, TClock>();
        }
    }
}
