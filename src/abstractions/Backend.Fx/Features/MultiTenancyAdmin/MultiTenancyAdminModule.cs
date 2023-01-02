using Backend.Fx.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.MultiTenancyAdmin
{
    internal class MultiTenancyAdminModule : IModule
    {
        private readonly ITenantRepository _tenantRepository;

        public MultiTenancyAdminModule(ITenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        public void Register(ICompositionRoot compositionRoot)
        {
            compositionRoot.Register(ServiceDescriptor.Singleton(_tenantRepository));
            compositionRoot.Register(ServiceDescriptor.Singleton<ITenantService, TenantService>());
        }
    }
}