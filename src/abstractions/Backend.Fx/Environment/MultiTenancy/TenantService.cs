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
        ITenantIdProvider IdProvider { get; }
        
        TenantId CreateTenant(TenantCreationParameters param);
        void ActivateTenant(TenantId tenantId);
        void DeactivateTenant(TenantId tenantId);
        void DeleteTenant(TenantId tenantId);
        
        Tenant[] GetActiveTenants();
        Tenant[] GetActiveDemonstrationTenants();
        Tenant[] GetActiveProductionTenants();
    }

    public interface ITenantIdProvider
    {
        TenantId[] GetActiveTenantIds();
        TenantId[] GetActiveDemonstrationTenantIds();
        TenantId[] GetActiveProductionTenantIds();
    }

    public class TenantService : ITenantService
    {
        private static readonly ILogger Logger = LogManager.Create<TenantService>();
        private readonly IMessageBus _messageBus;
        private readonly ITenantRepository _tenantRepository;

        public ITenantIdProvider IdProvider { get; }

        public TenantService(IMessageBus messageBus, ITenantRepository tenantRepository)
        {
            _messageBus = messageBus;
            _tenantRepository = tenantRepository;
            IdProvider = new TenantServiceTenantIdProvider(this);
        }

        public TenantId CreateTenant(TenantCreationParameters param)
        {
            Logger.Info($"Creating tenant: {param.Name}");
            return CreateTenant(param.Name, param.Description, param.IsDemonstrationTenant, param.Configuration);
        }

        public void ActivateTenant(TenantId tenantId)
        {
            Logger.Info($"Activating tenant: {tenantId}");
            Tenant tenant = _tenantRepository.GetTenant(tenantId);
            tenant.State = TenantState.Active;
            _tenantRepository.SaveTenant(tenant);
            _messageBus.Publish(new TenantActivated(tenant.Id, tenant.Name, tenant.Description, tenant.IsDemoTenant));
        }

        public void DeactivateTenant(TenantId tenantId)
        {
            Logger.Info($"Deactivating tenant: {tenantId}");
            Tenant tenant = _tenantRepository.GetTenant(tenantId);
            tenant.State = TenantState.Inactive;
            _tenantRepository.SaveTenant(tenant);
            _messageBus.Publish(new TenantDeactivated(tenant.Id, tenant.Name, tenant.Description, tenant.IsDemoTenant));
        }

        public void DeleteTenant(TenantId tenantId)
        {
            Logger.Info($"Deleting tenant: {tenantId}");
            _tenantRepository.DeleteTenant(tenantId);
        }

        public Tenant[] GetActiveTenants()
        {
            var activeTenantIds = _tenantRepository
                                  .GetTenants()
                                  .Where(t => t.State == TenantState.Active)
                                  .ToArray();
            Logger.Trace($"Active TenantIds: {string.Join(",",activeTenantIds.Select(t => t.ToString()))}");
            return activeTenantIds;
        }

        public Tenant[] GetActiveDemonstrationTenants()
        {
            var activeDemonstrationTenantIds = _tenantRepository
                                               .GetTenants()
                                               .Where(t => t.State == TenantState.Active && t.IsDemoTenant)
                                               .ToArray();
            Logger.Trace($"Active Demonstration TenantIds: {string.Join(",",activeDemonstrationTenantIds.Select(t => t.ToString()))}");
            return activeDemonstrationTenantIds;
        }

        public Tenant[] GetActiveProductionTenants()
        {
            var activeProductionTenantIds = _tenantRepository
                                            .GetTenants()
                                            .Where(t => t.State == TenantState.Active && !t.IsDemoTenant)
                                            .ToArray();
            Logger.Trace($"Active Production TenantIds: {string.Join(",",activeProductionTenantIds.Select(t => t.ToString()))}");
            return activeProductionTenantIds;
        }

        protected virtual TenantId CreateTenant([NotNull] string name, string description, bool isDemo, string configuration)
        {
            Logger.Info($"Creating Tenant {name}");
            
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            if (_tenantRepository.GetTenants().Any(t => t.Name != null && t.Name.ToLowerInvariant() == name.ToLowerInvariant()))
            {
                throw new ArgumentException($"There is already a tenant named {name}");
            }

            var tenant = new Tenant(name, description, isDemo) {Configuration = configuration};
            _tenantRepository.SaveTenant(tenant);
            var tenantId = new TenantId(tenant.Id);
            _messageBus.Publish(new TenantActivated(tenant.Id, tenant.Name, tenant.Description, tenant.IsDemoTenant));
            return tenantId;
        }
        
        private class TenantServiceTenantIdProvider : ITenantIdProvider
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
}