using System.Linq;

namespace Backend.Fx.Environment.MultiTenancy
{
    /// <summary>
    /// An implementation of <see cref="ITenantIdProvider"/> that uses a <see cref="ITenantService"/> directly. Keep in mind that this
    /// relies on direct access to the tenant service and therefore implicitly to the underlying tenant repository. When multiple
    /// applications do not share the same persistence mechanism or are distributed to multiple hosts, this implementation cannot be
    /// used, but must be implemented by a client to the master tenant service, probably using a remoting technology like RESTful Service,
    /// HTTP, gRPC or SOAP web service. 
    /// </summary>
    public class TenantServiceTenantIdProvider : ITenantIdProvider
    {
        private readonly ITenantService _tenantService;

        public TenantServiceTenantIdProvider(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        public TenantId[] GetActiveDemonstrationTenantIds()
        {
            return _tenantService.GetActiveDemonstrationTenants()
                                 .Select(t => new TenantId(t.Id))
                                 .ToArray();
        }

        public TenantId[] GetActiveProductionTenantIds()
        {
            return _tenantService.GetActiveProductionTenants()
                                 .Select(t => new TenantId(t.Id))
                                 .ToArray();
        }

        public TenantId[] GetActiveTenantIds()
        {
            return _tenantService.GetActiveTenants()
                                 .Select(t => new TenantId(t.Id))
                                 .ToArray();
        }
    }
}