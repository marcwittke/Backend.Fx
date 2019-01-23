using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Bootstrapping
{
    public class AnApplication : BackendFxApplication
    {
        public AnApplication(ICompositionRoot compositionRoot, IScopeManager scopeManager, ITenantManager tenantManager)
            : base(compositionRoot, scopeManager, tenantManager)
        {
        }

        protected override Task OnBoot()
        {
            var tenants = TenantManager.GetTenants();
            var prodTenantId = tenants.SingleOrDefault(t => t.Name == "prod")?.Id;
            ProdTenantId = prodTenantId == null 
                ? TenantManager.CreateProductionTenant("prod", "unit test created", true, new CultureInfo("en-US")) 
                : new TenantId(prodTenantId.Value);

            var demoTenantId = tenants.SingleOrDefault(t => t.Name == "demo")?.Id;
            DemoTenantId = demoTenantId == null
                ? TenantManager.CreateDemonstrationTenant("demo", "unit test created", true, new CultureInfo("en-US")) 
                : new TenantId(demoTenantId.Value);
            
            return Task.CompletedTask;
        }

        public TenantId ProdTenantId { get; private set; }

        public TenantId DemoTenantId { get; private set; }
    }
}