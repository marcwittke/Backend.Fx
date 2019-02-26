using System;
using System.Globalization;
using System.Linq;
using Backend.Fx.Logging;
using JetBrains.Annotations;

namespace Backend.Fx.Environment.MultiTenancy
{
    /// <summary>
    /// Encapsulates the management of tenants
    /// Note that this should not use repositories and other building blocks, but access the persistence layer directly
    /// </summary>
    public interface ITenantManager : IDisposable
    {
        event EventHandler<TenantId> TenantCreated;
        event EventHandler<TenantId> TenantActivated;

        TenantId[] GetTenantIds();
        Tenant[] GetTenants();
        Tenant GetTenant(TenantId id);
        Tenant FindTenant(TenantId tenantId);
        TenantId FindMatchingTenantId(string requestUri);
        TenantId CreateDemonstrationTenant(string name, string description, bool isDefault, CultureInfo defaultCultureInfo, string uriMatchingExpression = null);
        TenantId CreateProductionTenant(string name, string description, bool isDefault, CultureInfo defaultCultureInfo, string uriMatchingExpression = null);
        TenantId GetDefaultTenantId();
        void SaveTenant(Tenant tenant);
    }

    public abstract class TenantManager : ITenantManager
    {
        private static readonly ILogger Logger = LogManager.Create<TenantManager>();
        private readonly object _padlock = new object();

        public TenantId CreateDemonstrationTenant(string name, string description, bool isDefault, CultureInfo defaultCultureInfo, string uriMatchingExpression = null)
        {
            Logger.Info($"Creating demonstration tenant: {name}");
            return CreateTenant(name, description, true, isDefault, defaultCultureInfo, uriMatchingExpression);
        }

        public TenantId CreateProductionTenant(string name, string description, bool isDefault, CultureInfo defaultCultureInfo, string uriMatchingExpression = null)
        {
            Logger.Info($"Creating production tenant: {name}");
            return CreateTenant(name, description, false, isDefault, defaultCultureInfo, uriMatchingExpression);
        }

        public abstract TenantId FindMatchingTenantId(string requestUri);

        public abstract TenantId GetDefaultTenantId();

        public void SaveTenant(Tenant tenant)
        {
            var existingTenant = FindTenant(new TenantId(tenant.Id));
            lock (_padlock)
            {
                SaveTenantPersistent(existingTenant, tenant);
            }

            if (existingTenant == null)
            {
                TenantCreated?.Invoke(this, new TenantId(tenant.Id));
            }
            else
            {
                if (existingTenant.State != TenantState.Active && tenant.State == TenantState.Active)
                {
                    TenantActivated?.Invoke(this, new TenantId(tenant.Id));
                }
            }
        }

        protected abstract void SaveTenantPersistent(Tenant existingTenant, Tenant tenant);

        public event EventHandler<TenantId> TenantCreated;
        public event EventHandler<TenantId> TenantActivated;

        public abstract TenantId[] GetTenantIds();

        public abstract Tenant[] GetTenants();

        public abstract Tenant GetTenant(TenantId tenantId);

        public abstract Tenant FindTenant(TenantId tenantId);

        private TenantId CreateTenant([NotNull] string name, string description, bool isDemo, bool isDefault, CultureInfo defaultCultureInfo, string uriMatchingExpression)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            if (GetTenants().Any(t => t.Name != null && t.Name.ToLowerInvariant() == name.ToLowerInvariant()))
            {
                throw new ArgumentException($"There is already a tenant named {name}");
            }

            Tenant tenant = new Tenant(name, description, isDemo, defaultCultureInfo)
            {
                State = TenantState.Created,
                IsDefault = isDefault,
                UriMatchingExpression = uriMatchingExpression
            };
            SaveTenant(tenant);
            var tenantId = new TenantId(tenant.Id);
            return tenantId;
        }

        protected abstract void Dispose(bool disposing);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
