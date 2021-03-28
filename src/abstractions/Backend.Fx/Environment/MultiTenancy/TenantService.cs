using System;
using System.Linq;
using Backend.Fx.Exceptions;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.Environment.MultiTenancy
{
    /// <summary>
    /// Encapsulates the management of tenants
    /// Note that this should not use repositories and other building blocks, but access the persistence layer directly
    /// </summary>
    public interface ITenantService
    {
        /// <summary>
        /// The tenant service can also provide an <see cref="ITenantIdProvider"/>. Keep in mind that this instance uses a direct
        /// database connection. When multiple microservices do not share the same database, this instance cannot be used, but must
        /// be implemented by a client to the master tenant service, probably using a remoting technology like RESTful Service, HTTP,
        /// gRPC or SOAP web service 
        /// </summary>
        ITenantIdProvider TenantIdProvider { get; }

        TenantId CreateTenant(string name, string description, bool isDemonstrationTenant, string configuration = null);
        void ActivateTenant(TenantId tenantId);
        void DeactivateTenant(TenantId tenantId);
        void DeleteTenant(TenantId tenantId);

        Tenant[] GetTenants();
        Tenant[] GetActiveTenants();
        Tenant[] GetActiveDemonstrationTenants();
        Tenant[] GetActiveProductionTenants();
        Tenant GetTenant(TenantId tenantId);
    }

    public class TenantService : ITenantService
    {
        private static readonly ILogger Logger = LogManager.Create<TenantService>();
        private readonly IMessageBus _messageBus;
        private readonly ITenantRepository _tenantRepository;

        public ITenantIdProvider TenantIdProvider { get; }

        public TenantService(IMessageBus messageBus, ITenantRepository tenantRepository)
        {
            _messageBus = messageBus;
            _tenantRepository = tenantRepository;
            TenantIdProvider = new TenantServiceTenantIdProvider(this);
        }

        public TenantId CreateTenant(string name, string description, bool isDemonstrationTenant, string configuration = null)
        {
            Logger.Info($"Creating tenant: {name}");
            
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            if (_tenantRepository.GetTenants().Any(t => t.Name != null && t.Name.ToLowerInvariant() == name.ToLowerInvariant()))
            {
                throw new ArgumentException($"There is already a tenant named {name}");
            }

            var tenant = new Tenant(name, description, isDemonstrationTenant) {Configuration = configuration};
            _tenantRepository.SaveTenant(tenant);
            var tenantId = new TenantId(tenant.Id);
            _messageBus.Publish(new TenantActivated(tenant.Id, tenant.Name, tenant.Description, tenant.IsDemoTenant));
            return tenantId;
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
            Tenant tenant = _tenantRepository.GetTenant(tenantId);
            if (tenant.State != TenantState.Inactive)
            {
                throw new UnprocessableException($"Attempt to delete active tenant[{tenantId.Value}]")
                    .AddError("You cannot delete an active tenant. Please make sure to deactivate it first.");
            }

            _tenantRepository.DeleteTenant(tenantId);
        }

        public Tenant GetTenant(TenantId tenantId)
        {
            return _tenantRepository.GetTenant(tenantId);
        }

        public Tenant[] GetTenants()
        {
            var tenants = _tenantRepository.GetTenants();
            Logger.Trace($"TenantIds: {string.Join(",", tenants.Select(t => t.ToString()))}");
            return tenants;
        }

        public Tenant[] GetActiveTenants()
        {
            var activeTenants = _tenantRepository
                                  .GetTenants()
                                  .Where(t => t.State == TenantState.Active)
                                  .ToArray();
            Logger.Trace($"Active TenantIds: {string.Join(",", activeTenants.Select(t => t.ToString()))}");
            return activeTenants;
        }

        public Tenant[] GetActiveDemonstrationTenants()
        {
            var activeDemonstrationTenants = _tenantRepository
                                               .GetTenants()
                                               .Where(t => t.State == TenantState.Active && t.IsDemoTenant)
                                               .ToArray();
            Logger.Trace($"Active Demonstration TenantIds: {string.Join(",", activeDemonstrationTenants.Select(t => t.ToString()))}");
            return activeDemonstrationTenants;
        }

        public Tenant[] GetActiveProductionTenants()
        {
            var activeProductionTenants = _tenantRepository
                                            .GetTenants()
                                            .Where(t => t.State == TenantState.Active && !t.IsDemoTenant)
                                            .ToArray();
            Logger.Trace($"Active Production TenantIds: {string.Join(",", activeProductionTenants.Select(t => t.ToString()))}");
            return activeProductionTenants;
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