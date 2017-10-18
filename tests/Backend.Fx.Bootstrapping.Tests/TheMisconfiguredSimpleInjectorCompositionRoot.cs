namespace Backend.Fx.Bootstrapping.Tests
{
    using System;
    using BuildingBlocks;
    using Modules;
    using SimpleInjector;
    using Xunit;

    public class TheMisconfiguredSimpleInjectorCompositionRoot
    {
        [Fact]
        public void ThrowsOnValidation()
        {
            SimpleInjectorCompositionRoot sut = new SimpleInjectorCompositionRoot();
            sut.RegisterModules(new BadModule(sut));
            Assert.Throws<InvalidOperationException>(() => sut.Verify());
        }

        public class UnresolvableService
        {
            public UnresolvableService(Entity e)
            {
                throw new Exception("This constructor should never be called, since the Entity e cannot be resolved by the container");
            }
        }

        public class BadModule : SimpleInjectorModule
        {
            public BadModule(SimpleInjectorCompositionRoot compositionRoot) : base(compositionRoot)
            { }

            protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
            {
                // this registration should be recognized as unresolvable during validation
                container.Register<UnresolvableService>();
            }
        }
    }
}