using System.Reflection;
using Backend.Fx.SimpleInjectorDependencyInjection.Modules;
using Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Domain;
using SimpleInjector;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Bootstrapping
{
    public class ADomainModule : SimpleInjectorDomainModule
    {
        public ADomainModule(params Assembly[] domainAssemblies)
            : base(domainAssemblies)
        {
        }

        protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
        {
            base.Register(container, scopedLifestyle);
            container.Register<SomeState>(scopedLifestyle);
        }
    }
}
