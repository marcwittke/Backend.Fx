namespace Backend.Fx.Bootstrapping.Modules
{
    using System;
    using System.Linq;
    using System.Reflection;
    using BuildingBlocks;
    using Patterns.DependencyInjection;
    using Patterns.Jobs;
    using SimpleInjector;

    public class JobsModule : SimpleInjectorModule
    {
        private readonly Assembly[] assemblies;
        
        public JobsModule(SimpleInjectorCompositionRoot compositionRoot, params Assembly[] domainAssemblies) : base(compositionRoot)
        {
            assemblies = domainAssemblies.Concat(new[] {
                typeof(Entity).GetTypeInfo().Assembly,
            }).ToArray();
        }

        protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
        {
            // we have a singleton job executor
            container.RegisterSingleton<IJobExecutor, JobExecutor>();
            
            // all jobs are dynamically registered
            foreach (var scheduledJobType in container.GetTypesToRegister(typeof(IJob), assemblies))
            {
                container.Register(scheduledJobType);
            }
        }
    }
}
