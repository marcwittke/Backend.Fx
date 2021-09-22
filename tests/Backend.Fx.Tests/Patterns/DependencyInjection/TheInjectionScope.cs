using System;
using Backend.Fx.Patterns.DependencyInjection;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Patterns.DependencyInjection
{
    public class TheInjectionScope
    {
        private readonly IInstanceProvider _instanceProvider = A.Fake<IInstanceProvider>();

        [Fact]
        public void InitializesWithSequenceNumber()
        {
            var injectionScope = new TestInjectionScope(42, _instanceProvider);
            Assert.Equal(42, injectionScope.SequenceNumber);
        }

        [Fact]
        public void KeepsInstanceProvider()
        {
            var injectionScope = new TestInjectionScope(42, _instanceProvider);
            Assert.Equal(_instanceProvider, injectionScope.InstanceProvider);
        }


        private class TestInjectionScope : InjectionScope
        {
            public TestInjectionScope(int sequenceNumber, IInstanceProvider instanceProvider) : base(sequenceNumber)
            {
                InstanceProvider = instanceProvider;
            }

            public override IInstanceProvider InstanceProvider { get; }

            public override void Dispose()
            { }
        }
    }
}
