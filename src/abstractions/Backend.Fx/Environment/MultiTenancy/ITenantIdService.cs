using System.Linq;

namespace Backend.Fx.Environment.MultiTenancy
{
    public interface ITenantIdService 
    {
        TenantId[] GetActiveTenantIds();
        TenantId[] GetActiveDemonstrationTenantIds();
        TenantId[] GetActiveProductionTenantIds();
    }

    public class TenantIdService : ITenantIdService
    {
        private readonly ITenantRepository _tenantRepository;

        public TenantIdService(ITenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }


        public TenantId[] GetActiveTenantIds()
        {
            return _tenantRepository
                .GetTenants()
                .Where(t => t.State == TenantState.Active)
                .Select(t => new TenantId(t.Id))
                .ToArray();
        }

        public TenantId[] GetActiveDemonstrationTenantIds()
        {
            return _tenantRepository
                .GetTenants()
                .Where(t => t.State == TenantState.Active && t.IsDemoTenant)
                .Select(t => new TenantId(t.Id))
                .ToArray();
        }

        public TenantId[] GetActiveProductionTenantIds()
        {
            return _tenantRepository
                .GetTenants()
                .Where(t => t.State == TenantState.Active && !t.IsDemoTenant)
                .Select(t => new TenantId(t.Id))
                .ToArray();
        }
    }

}