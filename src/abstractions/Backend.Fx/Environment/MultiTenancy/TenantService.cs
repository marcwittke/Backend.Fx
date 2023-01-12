using System;
using System.Linq;
using Backend.Fx.Exceptions;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

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
        Tenant UpdateTenant(TenantId tenantId, string name, string description, string configuration);

        Tenant[] GetTenants();
        Tenant[] GetActiveTenants();
        Tenant[] GetActiveDemonstrationTenants();
        Tenant[] GetActiveProductionTenants();
        Tenant GetTenant(TenantId tenantId);
    }

    public class TenantService : ITenantService
    {
        private static readonly ILogger Logger = Log.Create<TenantService>();
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
            Logger.LogInformation("Creating tenant: {Name}", name);

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            if (_tenantRepository.GetTenants().Any(t => t.Name != null && t.Name.ToLowerInvariant() == name.ToLowerInvariant()))
            {
                throw new ArgumentException($"There is already a tenant named {name}");
            }

            var tenant = new Tenant(name, description, isDemonstrationTenant) { Configuration = configuration };
            _tenantRepository.SaveTenant(tenant);
            var tenantId = new TenantId(tenant.Id);
            var tenantActivated = new TenantActivated(tenant.Name, tenant.Description, tenant.IsDemoTenant);
            tenantActivated.SetTenantId(tenantId);
            _messageBus.Publish(tenantActivated);
            return tenantId;
        }

        public void ActivateTenant(TenantId tenantId)
        {
            Logger.LogInformation("Activating tenant: {TenantId}", tenantId);
            Tenant tenant = _tenantRepository.GetTenant(tenantId);
            tenant.State = TenantState.Active;
            _tenantRepository.SaveTenant(tenant);
            var tenantActivated = new TenantActivated(tenant.Name, tenant.Description, tenant.IsDemoTenant);
            tenantActivated.SetTenantId(tenantId);
            _messageBus.Publish(tenantActivated);
        }

        public void DeactivateTenant(TenantId tenantId)
        {
            Logger.LogInformation("Deactivating tenant: {TenantId}", tenantId);
            Tenant tenant = _tenantRepository.GetTenant(tenantId);
            tenant.State = TenantState.Inactive;
            _tenantRepository.SaveTenant(tenant);
            var tenantDeactivated = new TenantDeactivated(tenant.Name, tenant.Description, tenant.IsDemoTenant);
            tenantDeactivated.SetTenantId(tenantId);
            _messageBus.Publish(tenantDeactivated);
        }

        public void DeleteTenant(TenantId tenantId)
        {
            Logger.LogInformation("Deleting tenant: {TenantId}", tenantId);
            Tenant tenant = _tenantRepository.GetTenant(tenantId);
            if (tenant.State != TenantState.Inactive)
            {
                throw new UnprocessableException($"Attempt to delete active tenant[{tenantId.Value}]")
                    .AddError("You cannot delete an active tenant. Please make sure to deactivate it first.");
            }

            _tenantRepository.DeleteTenant(tenantId);
            var tenantDeactivated = new TenantDeactivated(tenant.Name, tenant.Description, tenant.IsDemoTenant);
            tenantDeactivated.SetTenantId(tenantId);
            _messageBus.Publish(tenantDeactivated);
        }

        public Tenant GetTenant(TenantId tenantId)
        {
            return _tenantRepository.GetTenant(tenantId);
        }

        public Tenant UpdateTenant(TenantId tenantId, string name, string description, string configuration)
        {
            var tenant = _tenantRepository.GetTenant(tenantId);
            tenant.Name = name;
            tenant.Description = description;
            tenant.Configuration = configuration;
            _tenantRepository.SaveTenant(tenant);
            var tenantUpdated = new TenantUpdated(tenant.Id, name, description, tenant.IsDemoTenant);
            tenantUpdated.SetTenantId(tenantId);
            _messageBus.Publish(tenantUpdated);
            return tenant;
        }

        public Tenant[] GetTenants()
        {
            var tenants = _tenantRepository.GetTenants();
            Logger.LogTrace("TenantIds: {TenantIds}", string.Join(",", tenants.Select(t => t.ToString())));
            return tenants;
        }

        public Tenant[] GetActiveTenants()
        {
            var activeTenants = _tenantRepository
                                .GetTenants()
                                .Where(t => t.State == TenantState.Active)
                                .ToArray();
            Logger.LogTrace("Active TenantIds: {TenantIds}", string.Join(",", activeTenants.Select(t => t.ToString())));
            return activeTenants;
        }

        public Tenant[] GetActiveDemonstrationTenants()
        {
            var activeDemonstrationTenants = _tenantRepository
                                             .GetTenants()
                                             .Where(t => t.State == TenantState.Active && t.IsDemoTenant)
                                             .ToArray();
            Logger.LogTrace("Active Demonstration TenantIds: {TenantIds}",
                string.Join(",", activeDemonstrationTenants.Select(t => t.ToString())));
            return activeDemonstrationTenants;
        }

        public Tenant[] GetActiveProductionTenants()
        {
            var activeProductionTenants = _tenantRepository
                                          .GetTenants()
                                          .Where(t => t.State == TenantState.Active && !t.IsDemoTenant)
                                          .ToArray();
            Logger.LogTrace("Active Production TenantIds: {TenantIds}",
                string.Join(",", activeProductionTenants.Select(t => t.ToString())));
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