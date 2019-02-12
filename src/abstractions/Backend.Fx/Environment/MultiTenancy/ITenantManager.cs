namespace Backend.Fx.Environment.MultiTenancy
{
    using System;
    using System.Globalization;
    using System.Linq;
    using JetBrains.Annotations;
    using Logging;

    /// <summary>
    /// Encapsulates the management of tenants
    /// Note that this should not use repositories and other building blocks, but access the persistence layer directly
    /// </summary>
    public interface ITenantManager  : IDisposable
    {
        event EventHandler<TenantId> TenantCreated;

        TenantId[] GetTenantIds();
        Tenant[] GetTenants();
        Tenant GetTenant(TenantId id);
        Tenant FindTenant(TenantId tenantId);
        TenantId CreateDemonstrationTenant(string name, string description, bool isDefault, CultureInfo defaultCultureInfo, string uriMatchingExpression = null);
        TenantId CreateProductionTenant(string name, string description, bool isDefault, CultureInfo defaultCultureInfo, string uriMatchingExpression = null);
        TenantId GetDefaultTenantId();
        void SaveTenant(Tenant tenant);
    }

    public abstract class TenantManager : ITenantManager
    {
        private static readonly ILogger Logger = LogManager.Create<TenantManager>();
        private readonly object _syncLock = new object();
        
        public TenantId CreateDemonstrationTenant(string name, string description, bool isDefault, CultureInfo defaultCultureInfo, string uriMatchingExpression = null)
        {
            lock (_syncLock)
            {
                Logger.Info($"Creating demonstration tenant: {name}");
                return CreateTenant(name, description, true, isDefault, defaultCultureInfo, uriMatchingExpression);
            }
        }

        public TenantId CreateProductionTenant(string name, string description, bool isDefault, CultureInfo defaultCultureInfo, string uriMatchingExpression = null)
        {
            lock (_syncLock)
            {
                Logger.Info($"Creating production tenant: {name}");
                return CreateTenant(name, description, false, isDefault, defaultCultureInfo, uriMatchingExpression);
            }
        }

        public TenantId GetDefaultTenantId()
        {
            var defaultTenant = GetTenants().SingleOrDefault(t => t.IsDefault);
            return defaultTenant == null
                       ? null
                       : new TenantId(defaultTenant.Id);
        }

        public abstract void SaveTenant(Tenant tenant);

        public event EventHandler<TenantId> TenantCreated;

        public abstract TenantId[] GetTenantIds();

        public abstract Tenant[] GetTenants();

        public Tenant GetTenant(TenantId tenantId)
        {
            Tenant tenant = FindTenant(tenantId);

            if (tenant == null)
            {
                throw new ArgumentException($"Invalid tenant Id [{tenantId.Value}]", nameof(tenantId));
            }

            return tenant;
        }

        public abstract Tenant FindTenant(TenantId tenantId);

        private TenantId CreateTenant([NotNull] string name, string description, bool isDemo, bool isDefault, CultureInfo defaultCultureInfo, string uriMatchingExpression)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

            if (GetTenants().Any(t => t.Name != null && t.Name.ToLowerInvariant() == name.ToLowerInvariant()))
            {
                throw new ArgumentException($"There is already a tenant named {name}");
            }

            Tenant tenant = new Tenant(name, description, isDemo, defaultCultureInfo) { State = TenantState.Created, IsDefault = isDefault, UriMatchingExpression = uriMatchingExpression};
            SaveTenant(tenant);
            var tenantId = new TenantId(tenant.Id);
            TenantCreated?.Invoke(this, tenantId);
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
