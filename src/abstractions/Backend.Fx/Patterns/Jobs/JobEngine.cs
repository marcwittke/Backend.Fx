using System.Threading.Tasks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Patterns.Jobs
{
    public class JobEngine : IJobEngine
    {
        private static readonly ILogger Logger = LogManager.Create<JobEngine>();
        private readonly IScopeManager _scopeManager;
        
        public JobEngine(IScopeManager scopeManager)
        {
            _scopeManager = scopeManager;
        }

        public void ExecuteJob<TJob>(TenantId tenantId) where TJob : IJob
        {
            using (Logger.InfoDuration($"Execution of {typeof(TJob).Name} for tenant[{(tenantId.HasValue ? tenantId.Value.ToString() : "null")}]"))
            {
                using (var scope = _scopeManager.BeginScope(new SystemIdentity(), tenantId))
                {
                    scope.GetInstance<IJobExecutor<TJob>>().ExecuteJob();
                }
            }
        }
        
        public Task ExecuteJobAsync<TJob>(TenantId tenantId) where TJob : IJob
        {
            return Task.Factory.StartNew(() => ExecuteJob<TJob>(tenantId));
        }
    }
}