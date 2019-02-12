using System.Reflection;
using Backend.Fx.Logging;
using Backend.Fx.SimpleInjectorDependencyInjection.Modules;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Bootstrapping
{
    public class ADomainModule : DomainModule
    {
        public ADomainModule(params Assembly[] domainAssemblies) : base(new DebugExceptionLogger(), domainAssemblies)
        {}
    }
}
