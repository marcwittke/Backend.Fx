namespace DemoBlog.Application.Jobs
{
    using System;
    using Backend.Fx.Environment.Authentication;
    using Backend.Fx.Environment.MultiTenancy;
    using Backend.Fx.Patterns.DependencyInjection;
    using Backend.Fx.Patterns.Jobs;
    using JetBrains.Annotations;

    /// <summary>
    /// Wraps a pure Application Job (implementing <see cref="FluentScheduler.IJob"/>) in a 
    /// Fluent Scheduler Job (implementing <see cref="FluentScheduler.IJob"/>) to make it schedulable
    /// </summary>
    public class ApplicationRuntimeJobWrapper : FluentScheduler.IJob
    {
        private readonly IScopeManager scopeManager;
        private readonly ITenantManager tenantManager;
        private readonly Type jobType;

        [UsedImplicitly]
        public ApplicationRuntimeJobWrapper(IScopeManager scopeManager, ITenantManager tenantManager, Type jobType)
        {
            this.scopeManager = scopeManager;
            this.tenantManager = tenantManager;
            this.jobType = jobType;
        }

        [UsedImplicitly]
        public void Execute()
        {
            foreach (var tenantId in tenantManager.GetTenantIds())
            {
                using (var scope = scopeManager.BeginScope(new SystemIdentity(), tenantId))
                {
                    var jobExecutorType = typeof(IJobExecutor<>).MakeGenericType(jobType);
                    IJobExecutor jobExecutor = (IJobExecutor) scope.GetInstance(jobExecutorType);
                    jobExecutor.ExecuteJob();
                }
            }
        }
    }

    public class ApplicationRuntimeJobWrapper<TJob> : ApplicationRuntimeJobWrapper
    {
        public ApplicationRuntimeJobWrapper(IScopeManager scopeManager, ITenantManager tenantManager)
                : base(scopeManager, tenantManager, typeof(TJob))
        { }
    }
}