using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Backend.Fx.AspNetCore.Mvc.Activators;
using Backend.Fx.DependencyInjection;
using Backend.Fx.Util;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.AspNetCore.SignalR;

public class AspNetSignalRModule : IModule
{
    private readonly IServiceCollection _frameworkServices;
    private readonly IEnumerable<Assembly> _assemblies;

    public AspNetSignalRModule(IServiceCollection frameworkServices, IEnumerable<Assembly> assemblies)
    {
        _frameworkServices = frameworkServices;
        _assemblies = assemblies;
    }
    public void Register(ICompositionRoot compositionRoot)
    {
        foreach (var hubType in _assemblies.GetImplementingTypes<Hub>())
        {
            // register the singleton hub instance
            compositionRoot.Register(new ServiceDescriptor(hubType, hubType, ServiceLifetime.Singleton));
            
            // register a respective hub factory in the framework service collection
            Type hubActivatorServiceType = typeof(IHubActivator<>).MakeGenericType(hubType);
            Type hubActivatorImplementationType = typeof(BackendFxApplicationHubActivator<>).MakeGenericType(hubType);
            object hubActivatorImplementation = Activator.CreateInstance(hubActivatorImplementationType, compositionRoot);
            Debug.Assert(hubActivatorImplementation != null, nameof(hubActivatorImplementation) + " != null");
            _frameworkServices.AddSingleton(hubActivatorServiceType, hubActivatorImplementation);
        }
    }
}