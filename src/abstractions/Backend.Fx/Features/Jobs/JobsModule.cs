using System;
using System.Reflection;
using Backend.Fx.DependencyInjection;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.Jobs
{
    internal class JobsModule : IModule
    {
        private readonly Assembly[] _assemblies;

        public JobsModule(Assembly[] assemblies)
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
            
            compositionRoot.Register(
                ServiceDescriptor.Singleton<IJobExecutor, JobExecutor>());
        }
    }

    internal class MultiTenancyJobsModule : IModule
    {
        public void Register(ICompositionRoot compositionRoot)
        {
            compositionRoot.RegisterDecorator(
                ServiceDescriptor.Singleton<IJobExecutor, ForEachTenantJobExecutor>());
        }
    }
}