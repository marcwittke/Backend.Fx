using System;
using System.Linq;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.EventAggregation.Integration;
using JetBrains.Annotations;

namespace Backend.Fx.Environment.MultiTenancy
{
    /// <summary>
    /// Encapsulates the management of tenants
    /// Note that this should not use repositories and other building blocks, but access the persistence layer directly
    /// </summary>
    public interface ITenantService
    {
        TenantId CreateDemonstrationTenant(string name, string description, string defaultCultureName);
        TenantId CreateProductionTenant(string name, string description, string defaultCultureName);
        void ActivateTenant(TenantId tenantId);
        void DeactivateTenant(TenantId tenantId);
    }

    public class TenantService : ITenantService
    {
        private readonly IEventBus _eventBus;
        private readonly ITenantRepository _tenantRepository;
        private static readonly ILogger Logger = LogManager.Create<TenantService>();
        
        public TenantService(IEventBus eventBus, ITenantRepository tenantRepository)
        {
            _eventBus = eventBus;
            _tenantRepository = tenantRepository;
        }

        public TenantId CreateDemonstrationTenant(string name, string description, string defaultCultureName)
        {
            Logger.Info($"Creating demonstration tenant: {name}");
            return CreateTenant(name, description, true, defaultCultureName);
        }

        public TenantId CreateProductionTenant(string name, string description, string defaultCultureName)
        {
            Logger.Info($"Creating production tenant: {name}");
            return CreateTenant(name, description, false, defaultCultureName);
        }
        
        public void ActivateTenant(TenantId tenantId)
        {
            var tenant = _tenantRepository.GetTenant(tenantId);
            tenant.State = TenantState.Active;
            _tenantRepository.SaveTenant(tenant);
            _eventBus.Publish(new TenantActivated(tenant.Id, tenant.Name, tenant.Description, tenant.IsDemoTenant, tenant.DefaultCultureName));
        }

        public void DeactivateTenant(TenantId tenantId)
        {
            var tenant = _tenantRepository.GetTenant(tenantId);
            tenant.State = TenantState.Inactive;
            _tenantRepository.SaveTenant(tenant);
            _eventBus.Publish(new TenantDeactivated(tenant.Id, tenant.Name, tenant.Description, tenant.IsDemoTenant, tenant.DefaultCultureName));
        }
        
        protected virtual TenantId CreateTenant([NotNull] string name, string description, bool isDemo, string defaultCultureName)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            if (_tenantRepository.GetTenants().Any(t => t.Name != null && t.Name.ToLowerInvariant() == name.ToLowerInvariant()))
            {
                throw new ArgumentException($"There is already a tenant named {name}");
            }

            Tenant tenant = new Tenant(name, description, isDemo, defaultCultureName);
            _tenantRepository.SaveTenant(tenant);
            var tenantId = new TenantId(tenant.Id);
            _eventBus.Publish(new TenantCreated(tenant.Id, tenant.Name, tenant.Description, tenant.IsDemoTenant, tenant.DefaultCultureName));
            return tenantId;
        }
    }
}
