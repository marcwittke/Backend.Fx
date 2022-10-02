using System;
using System.Collections.Generic;
using Backend.Fx.DependencyInjection;
using Backend.Fx.TestUtil;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.DependencyInjection;

public class TheCompositionRoot : TestWithLogging
{
    public TheCompositionRoot(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void RegistersModules()
    {
        var fakeModule = A.Fake<IModule>();
        using var sut = new DummyCompositionRoot();
        sut.RegisterModules(fakeModule);
        A.CallTo(() => fakeModule.Register(A<ICompositionRoot>.That.IsSameAs(sut)))
            .MustHaveHappenedOnceExactly();
    }

    private class DummyCompositionRoot : CompositionRoot
    {
        private ICompositionRoot FakeCompositionRoot { get; } = A.Fake<ICompositionRoot>();

        public override IServiceScope BeginScope()
        {
            return FakeCompositionRoot.BeginScope();
        }

        protected override void Dispose(bool disposing)
        {
        }

        public override IServiceProvider ServiceProvider => FakeCompositionRoot.ServiceProvider;

        public override void Verify()
        {
            FakeCompositionRoot.Verify();
        }

        public override void Register(ServiceDescriptor serviceDescriptor)
        {
            FakeCompositionRoot.Register(serviceDescriptor);
        }

        public override void RegisterDecorator(ServiceDescriptor serviceDescriptor)
        {
            FakeCompositionRoot.RegisterDecorator(serviceDescriptor);
        }

        public override void RegisterCollection(IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            FakeCompositionRoot.RegisterCollection(serviceDescriptors);
        }
    }
}