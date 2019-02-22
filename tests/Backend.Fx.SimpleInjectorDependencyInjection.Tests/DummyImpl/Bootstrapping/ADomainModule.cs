using System.Reflection;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.SimpleInjectorDependencyInjection.Modules;
using FakeItEasy;
using SimpleInjector;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Bootstrapping
{
    public class ADomainModule : DomainModule
    {
        public ADomainModule(params Assembly[] domainAssemblies) : base(new DebugExceptionLogger(), domainAssemblies)
        {}

        protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
        {
            base.Register(container, scopedLifestyle);
            container.RegisterInstance(A.Fake<IEventBus>());
        }
    }
}
