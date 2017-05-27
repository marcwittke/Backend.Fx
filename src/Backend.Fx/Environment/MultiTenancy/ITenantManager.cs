namespace Backend.Fx.Environment.MultiTenancy
{
    using System;
    using System.Linq;
    using JetBrains.Annotations;
    using Logging;

    public interface ITenantManager
    {
        TenantId[] GetTenantIds();
        Tenant[] GetTenants();
        bool IsActive(TenantId tenantId);
        TenantId CreateDemonstrationTenant(string name, string description);
        TenantId CreateProductionTenant(string name, string description);
        void EnsureTenantIsInitialized(TenantId tenantId);
    }

    public abstract class TenantManager : ITenantManager
    {
        private static readonly ILogger Logger = LogManager.Create<TenantManager>();
        private readonly ITenantInitializer tenantInitializer;
        private readonly object syncLock = new object();

        protected TenantManager(ITenantInitializer tenantInitializer)
        {
            this.tenantInitializer = tenantInitializer;
        }

        public TenantId CreateDemonstrationTenant(string name, string description)
        {
            lock (syncLock)
            {
                Logger.Info($"Creating demonstration tenant: {name}");
                return CreateTenant(name, description, true);
            }
        }

        public TenantId CreateProductionTenant(string name, string description)
        {
            Logger.Info($"Creating production tenant: {name}");
            lock (syncLock)
            {
                return CreateTenant(name, description, false);
            }
        }

        protected void InitializeTenant(Tenant tenant)
        {
            if (tenant.IsInitialized) return;

            Logger.Info($"Initializing {(tenant.IsDemoTenant ? "demonstration" : "production")} tenant[{tenant.Id}] ({tenant.Name})");
            var tenantId = new TenantId(tenant.Id);
            tenantInitializer.RunProductiveInitialDataGenerators(tenantId);
            if (tenant.IsDemoTenant)
            {
                tenantInitializer.RunDemoInitialDataGenerators(tenantId);
            }
        }

        public abstract TenantId[] GetTenantIds();

        public abstract Tenant[] GetTenants();

        public bool IsActive(TenantId tenantId)
        {
            Tenant tenant = FindTenant(tenantId);

            if (tenant == null)
            {
                throw new ArgumentException($"Invalid tenant Id [{tenantId.Value}]", nameof(tenantId));
            }

            return tenant.IsActive;
        }

        public void EnsureTenantIsInitialized(TenantId tenantId)
        {
            Tenant tenant = FindTenant(tenantId);

            if (tenant == null)
            {
                throw new ArgumentException($"Invalid tenant Id [{tenantId.Value}]", nameof(tenantId));
            }

            InitializeTenant(tenant);

            tenant.IsInitialized = true;
            SaveTenant(tenant);
        }

        protected abstract Tenant FindTenant(TenantId tenantId);

        private TenantId CreateTenant([NotNull] string name, string description, bool isDemo)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

            if (GetTenants().Any(t => t.Name != null && t.Name.ToLowerInvariant() == name.ToLowerInvariant()))
            {
                throw new ArgumentException($"There is already a tenant named {name}");
            }

            Tenant tenant = new Tenant(name, description, isDemo) { IsActive = true };
            SaveTenant(tenant);
            InitializeTenant(tenant);
            return new TenantId(tenant.Id);
        }

        protected abstract void SaveTenant(Tenant tenant);
    }
}
