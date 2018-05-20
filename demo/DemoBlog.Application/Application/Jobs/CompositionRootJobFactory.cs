namespace DemoBlog.Application.Jobs
{
    using System;
    using Backend.Fx.Environment.MultiTenancy;
    using Backend.Fx.Logging;
    using Backend.Fx.Patterns.DependencyInjection;
    using FluentScheduler;

    /// <summary>
    /// Factory to enable FluentScheduler to use our application runtime
    /// </summary>
    public class ApplicationRuntimeJobFactory : IJobFactory
    {
        private static readonly ILogger Logger = LogManager.Create<ApplicationRuntimeJobFactory>();
        private readonly IScopeManager scopeManager;
        private readonly ITenantManager tenantManager;
        
        public ApplicationRuntimeJobFactory(IScopeManager scopeManager, ITenantManager tenantManager)
        {
            this.scopeManager = scopeManager;
            this.tenantManager = tenantManager;
        }

        public IJob GetJobInstance<T>() where T : IJob
        {
            try
            {
                IJob job = (IJob) Activator.CreateInstance(typeof(T), scopeManager, tenantManager);
                return job;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Unable to create an instance of {typeof(T).Name}. Are you sure it implements ApplicationRuntimeJobWrapper<T>?");
                throw;
            }

            
        }
    }
}