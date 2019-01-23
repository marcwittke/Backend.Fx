using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;

namespace Backend.Fx.Patterns.Jobs
{
    public interface IJobEngine
    {
        void ExecuteJob<TJob>(TenantId tenantId) where TJob : IJob;
        
        Task ExecuteJobAsync<TJob>(TenantId tenantId) where TJob : IJob;
    }
}
