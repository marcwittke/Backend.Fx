using System.Collections.Generic;
using System.Reflection;
using Backend.Fx.DependencyInjection;
using Backend.Fx.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.AspNetCore.Bootstrapping
{
    public class AspNetCoreModule : IModule
    {
        private readonly IEnumerable<Assembly> _assemblies;

        public AspNetCoreModule(IEnumerable<Assembly> assemblies)
        {
            _assemblies = assemblies;
        }
        public void Register(ICompositionRoot compositionRoot)
        {
            RegisterAllImplementationsOf<ControllerBase>(compositionRoot);
            RegisterAllImplementationsOf<ViewComponent>(compositionRoot);
            RegisterAllImplementationsOf<Hub>(compositionRoot);
        }

        private void RegisterAllImplementationsOf<T>(ICompositionRoot compositionRoot)
        {
            foreach (var controllerType in _assemblies.GetImplementingTypes<T>())
            {
                compositionRoot.Register(
                    new ServiceDescriptor(
                        controllerType,
                        controllerType,
                        ServiceLifetime.Scoped));
            }
        }
    }
}