using System;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.SimpleInjectorDependencyInjection.Modules;
using Backend.Fx.Tests;
using SimpleInjector;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests
{
    public class TheMisconfiguredSimpleInjectorCompositionRoot : TestWithLogging
    {
        [Fact]
        public void ThrowsOnValidation()
        {
            var sut = new SimpleInjectorCompositionRoot();
            sut.RegisterModules(new BadModule());
            Assert.Throws<InvalidOperationException>(() => sut.Verify());
        }

        public class UnresolvableService
        {
            public UnresolvableService(Entity e)
            {
                throw new Exception(
                    $"This constructor should never be called, since the Entity {e?.GetType().Name} cannot be resolved by the container");
            }
        }

        public class BadModule : SimpleInjectorModule
        {
            protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
            {
                // this registration should be recognized as unresolvable during validation
                container.Register<UnresolvableService>();
            }
        }

        public TheMisconfiguredSimpleInjectorCompositionRoot(ITestOutputHelper output) : base(output)
        {
        }
    }
}