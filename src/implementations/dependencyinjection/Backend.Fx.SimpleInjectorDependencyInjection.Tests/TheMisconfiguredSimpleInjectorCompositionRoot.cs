using System;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Patterns.EventAggregation.Integration;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests
{
    public class TheMisconfiguredSimpleInjectorCompositionRoot
    {
        [Fact]
        public void ThrowsOnValidation()
        {
            var sut = new SimpleInjectorCompositionRoot(A.Fake<IMessageBus>());
            sut.Container.Register<UnresolvableService>();
            Assert.Throws<InvalidOperationException>(() => sut.Verify());
        }

        public class UnresolvableService
        {
            public UnresolvableService(Entity e)
            {
                throw new Exception($"This constructor should never be called, since the Entity {e?.GetType().Name} cannot be resolved by the container");
            }
        }
   }
}