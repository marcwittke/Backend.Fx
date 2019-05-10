using System.Reflection;
using Backend.Fx.SimpleInjectorDependencyInjection.Modules;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Bootstrapping
{
    public class ADomainModule : DomainModule
    {
        public ADomainModule(params Assembly[] domainAssemblies) 
            : base(domainAssemblies)
        {}
    }
}
