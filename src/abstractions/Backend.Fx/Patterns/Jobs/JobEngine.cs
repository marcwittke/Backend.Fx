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
        private readonly IBackendFxApplication _application;

        public JobEngine(IBackendFxApplication application)
        {
            _application = application;
        }

        public void ExecuteJob<TJob>() where TJob : IJob
        {
            var tenants = _application.TenantManager.GetTenants();
            foreach (var tenant in tenants)
            {
                if (tenant.State == TenantState.Active)
                {
                    ExecuteJob<TJob>(new TenantId(tenant.Id));
                }
            }
        }

        public async Task ExecuteJobAsync<TJob>() where TJob : IJob
        {
            var tenants = _application.TenantManager.GetTenants();
            foreach (var tenant in tenants)
            {
                if (tenant.State == TenantState.Active)
                {
                    await ExecuteJobAsync<TJob>(new TenantId(tenant.Id));
                }
            }
        }

        public void ExecuteJob<TJob>(TenantId tenantId) where TJob : IJob
        {
            using (Logger.InfoDuration($"Execution of {typeof(TJob).Name} for tenant[{(tenantId.HasValue ? tenantId.Value.ToString() : "null")}]"))
            {
                _application.Invoke(() =>
                {
                    _application.CompositionRoot.GetInstance<IJobExecutor<TJob>>().ExecuteJob();
                }, new SystemIdentity(), tenantId);
            }
        }

        public async Task ExecuteJobAsync<TJob>(TenantId tenantId) where TJob : IJob
        {
            using (Logger.InfoDuration($"Execution of {typeof(TJob).Name} for tenant[{(tenantId.HasValue ? tenantId.Value.ToString() : "null")}]"))
            {
                await _application.InvokeAsync(() =>
                {
                    _application.CompositionRoot.GetInstance<IJobExecutor<TJob>>().ExecuteJob();
                }, new SystemIdentity(), tenantId);
            }
        }
    }
}