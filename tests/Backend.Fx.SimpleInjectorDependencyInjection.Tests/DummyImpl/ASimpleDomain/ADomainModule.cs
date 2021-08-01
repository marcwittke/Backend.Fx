using System.Reflection;
using Backend.Fx.SimpleInjectorDependencyInjection.Modules;
using SimpleInjector;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.ASimpleDomain
{
    public class ADomainModule : SimpleInjectorDomainModule
    {
        public ADomainModule(params Assembly[] assemblies)
            : base(assemblies)
        {
        }

        protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
        {
            base.Register(container, scopedLifestyle);
            container.RegisterSingleton<ISingletonService, ASingletonService>();
            container.Register<SomeState>(scopedLifestyle);
        }
    }
}
