namespace Backend.Fx.Patterns.Jobs
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DependencyInjection;
    using Environment.Authentication;
    using Environment.MultiTenancy;
    using Logging;
    using UnitOfWork;

    /// <summary>
    /// Abstracts the functionality of the framework to run a job in background. This can be triggered 
    /// by a schedulter, or, also be done manually
    /// </summary>
    public interface IJobExecutor
    {
        Task ExecuteJobAsync<TJob>(int? tenantId = null, int delayInSeconds = 0) where TJob : class, IJob;
    }

    public class JobExecutor : IJobExecutor
    {
        private readonly ITenantManager tenantManager;
        private readonly IScopeManager scopeManager;
        private static readonly ILogger Logger = LogManager.Create<JobExecutor>();

        public JobExecutor(ITenantManager tenantManager, IScopeManager scopeManager)
        {
            this.tenantManager = tenantManager;
            this.scopeManager = scopeManager;
        }

        public virtual void ExecuteJob<TJob>(int? tenantId = null, int delayInSeconds = 0) where TJob : class, IJob
        {
            Thread.Sleep(delayInSeconds * 1000);
            TenantId[] tenants = tenantId == null
                                         ? tenantManager.GetTenantIds()
                                         : new[] { new TenantId(tenantId.Value) };

            string jobName = typeof(TJob).Name;
            foreach (var tenant in tenants)
            {
                using (Logger.InfoDuration($"Beginning {jobName} scope", $"{jobName} scope completed"))
                {
                    using (IScope scope = scopeManager.BeginScope(new SystemIdentity(), tenant))
                    {
                        scope.BeginUnitOfWork(false);
                        try
                        {
                            scope.GetInstance<TJob>().Execute();
                            scope.GetInstance<IUnitOfWork>().Complete();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, $"Execution of {jobName} failed: {ex.Message}");
                        }
                        finally
                        {
                            scope.GetInstance<IUnitOfWork>().Dispose();
                        }
                    }
                }
            }
        }

        public virtual async Task ExecuteJobAsync<TJob>(int? tenantId = null, int delayInSeconds = 0) where TJob : class, IJob
        {
            await Task.Run(() => ExecuteJob<TJob>(tenantId, delayInSeconds));
        }
    }
}
