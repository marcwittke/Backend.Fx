using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.MultiTenancyAdmin
{
    [PublicAPI]
    public class MultiTenancyAdminFeature : Feature, IBootableFeature
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly bool _ensureDemoTenantOnBoot;

        public MultiTenancyAdminFeature(ITenantRepository tenantRepository, bool ensureDemoTenantOnBoot = false)
        {
            _tenantRepository = tenantRepository;
            _ensureDemoTenantOnBoot = ensureDemoTenantOnBoot;
        }
        
        public override void Enable(IBackendFxApplication application)
        {
            application.CompositionRoot.RegisterModules(new MultiTenancyAdminModule(_tenantRepository));
        }

        public Task BootAsync(IBackendFxApplication application, CancellationToken cancellationToken = default)
        {
            if (_ensureDemoTenantOnBoot)
            {
                var tenantService = application.CompositionRoot.ServiceProvider.GetRequiredService<ITenantService>();
                if (!tenantService.GetTenants().Any(t => t.IsDemoTenant))
                {
                    tenantService.CreateTenant("demo", "Demonstration Tenant", true);
                }
            }

            return Task.CompletedTask;
        }
    }
}