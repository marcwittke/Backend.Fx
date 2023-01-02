using System;
using System.Linq;
using Backend.Fx.Exceptions;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Features.MultiTenancyAdmin
{
    /// <summary>
    /// Encapsulates the management of tenants
    /// </summary>
    [PublicAPI]
    public interface ITenantService
    {
        Tenant CreateTenant(string name, string description, bool isDemonstrationTenant, string configuration = null);
        void ActivateTenant(int tenantId);
        void DeactivateTenant(int tenantId);
        void DeleteTenant(int tenantId);
        Tenant UpdateTenant(int tenantId, string name, string description, string configuration);

        Tenant[] GetTenants();
        Tenant[] GetActiveTenants();
        Tenant[] GetActiveDemonstrationTenants();
        Tenant[] GetActiveProductionTenants();
        Tenant GetTenant(int tenantId);
    }

    public class TenantService : ITenantService
    {
        private readonly ILogger _logger = Log.Create<TenantService>();
        private readonly ITenantRepository _tenantRepository;

        public TenantService(ITenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        public Tenant CreateTenant(string name, string description, bool isDemonstrationTenant,
            string configuration = null)
        {
            _logger.LogInformation("Creating tenant: {Name}", name);

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            if (_tenantRepository.GetTenants()
                .Any(t => t.Name != null && string.Equals(t.Name, name)))
            {
                throw new ArgumentException($"There is already a tenant named {name}");
            }

            var tenant = new Tenant(_tenantRepository.GetNextTenantId(), name, description, isDemonstrationTenant) { Configuration = configuration };
            _tenantRepository.SaveTenant(tenant);

            return tenant;
        }

        public void ActivateTenant(int tenantId)
        {
            _logger.LogInformation("Activating tenant: {TenantId}", tenantId);
            Tenant tenant = _tenantRepository.GetTenant(tenantId);
            tenant.IsActive = true;
            _tenantRepository.SaveTenant(tenant);
        }

        public void DeactivateTenant(int tenantId)
        {
            _logger.LogInformation("Deactivating tenant: {TenantId}", tenantId);
            Tenant tenant = _tenantRepository.GetTenant(tenantId);
            tenant.IsActive = false;
            _tenantRepository.SaveTenant(tenant);
        }

        public void DeleteTenant(int tenantId)
        {
            _logger.LogInformation("Deleting tenant: {TenantId}", tenantId);
            Tenant tenant = _tenantRepository.GetTenant(tenantId);
            if (tenant.IsActive)
            {
                throw new UnprocessableException($"Attempt to delete active tenant[{tenantId}]")
                    .AddError("You cannot delete an active tenant. Please make sure to deactivate it first.");
            }

            _tenantRepository.DeleteTenant(tenantId);
        }

        public Tenant GetTenant(int tenantId)
        {
            return _tenantRepository.GetTenant(tenantId);
        }

        public Tenant UpdateTenant(int tenantId, string name, string description, string configuration)
        {
            Tenant tenant = _tenantRepository.GetTenant(tenantId);
            tenant.Name = name;
            tenant.Description = description;
            tenant.Configuration = configuration;
            _tenantRepository.SaveTenant(tenant);
            return tenant;
        }

        public Tenant[] GetTenants()
        {
            var tenants = _tenantRepository.GetTenants();
            _logger.LogTrace("TenantIds: {TenantIds}", string.Join(",", tenants.Select(t => t.ToString())));
            return tenants;
        }

        public Tenant[] GetActiveTenants()
        {
            var activeTenants = _tenantRepository
                .GetTenants()
                .Where(t => t.IsActive)
                .ToArray();
            _logger.LogTrace("Active TenantIds: {TenantIds}", string.Join(",", activeTenants.Select(t => t.ToString())));
            return activeTenants;
        }

        public Tenant[] GetActiveDemonstrationTenants()
        {
            var activeDemonstrationTenants = _tenantRepository
                .GetTenants()
                .Where(t => t.IsActive && t.IsDemoTenant)
                .ToArray();
            _logger.LogTrace("Active Demonstration TenantIds: {TenantIds}",
                string.Join(",", activeDemonstrationTenants.Select(t => t.ToString())));
            return activeDemonstrationTenants;
        }

        public Tenant[] GetActiveProductionTenants()
        {
            var activeProductionTenants = _tenantRepository
                .GetTenants()
                .Where(t => t.IsActive && !t.IsDemoTenant)
                .ToArray();
            _logger.LogTrace("Active Production TenantIds: {TenantIds}",
                string.Join(",", activeProductionTenants.Select(t => t.ToString())));
            return activeProductionTenants;
        }
    }
}