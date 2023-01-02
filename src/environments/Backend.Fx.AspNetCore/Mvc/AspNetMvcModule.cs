using System.Collections.Generic;
using System.Reflection;
using Backend.Fx.DependencyInjection;
using Backend.Fx.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.AspNetCore.Mvc;

public class AspNetMvcModule : IModule
{
    private readonly IEnumerable<Assembly> _assemblies;

    public AspNetMvcModule(IEnumerable<Assembly> assemblies)
    {
        _assemblies = assemblies;
    }
    public void Register(ICompositionRoot compositionRoot)
    {
        RegisterAllImplementationsOf<ControllerBase>(compositionRoot);
        RegisterAllImplementationsOf<ViewComponent>(compositionRoot);
    }

    private void RegisterAllImplementationsOf<T>(ICompositionRoot compositionRoot)
    {
        foreach (var implementationType in _assemblies.GetImplementingTypes<T>())
        {
            compositionRoot.Register(
                new ServiceDescriptor(
                    implementationType,
                    implementationType,
                    ServiceLifetime.Scoped));
        }
    }
}