using System;
using System.Reflection;
using Backend.Fx.DependencyInjection;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.Jobs
{
    internal class JobModule : IModule
    {
        private readonly Assembly[] _assemblies;

        public JobModule(Assembly[] assemblies)
        {
            _assemblies = assemblies;
        }

        public void Register(ICompositionRoot compositionRoot)
        {
            // all jobs are dynamically registered
            foreach (Type jobType in _assemblies.GetImplementingTypes(typeof(IJob)))
            {
                compositionRoot.Register(
                    new ServiceDescriptor(jobType, jobType, ServiceLifetime.Scoped));
            }
        }
    }
}