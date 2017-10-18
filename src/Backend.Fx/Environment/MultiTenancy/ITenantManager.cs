﻿namespace Backend.Fx.Environment.MultiTenancy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using Logging;

    /// <summary>
    /// Encapsulates the management of tenants
    /// Note that this should not use repositories and other building blocks, but access the persistence layer directly
    /// </summary>
    public interface ITenantManager
    {
        TenantId[] GetTenantIds();
        Tenant[] GetTenants();
        bool IsActive(TenantId tenantId);
        void EnsureTenantIsInitialized(TenantId tenantId);
        Tenant FindTenant(TenantId tenantId);
        TenantId CreateDemonstrationTenant(string name, string description, bool isDefault);
        TenantId CreateProductionTenant(string name, string description, bool isDefault);

    }

    public abstract class TenantManager : ITenantManager
    {
        private static readonly ILogger Logger = LogManager.Create<TenantManager>();
        private readonly ITenantInitializer tenantInitializer;
        private readonly object syncLock = new object();
        private readonly HashSet<int> initializedTenants = new HashSet<int>();

        protected TenantManager(ITenantInitializer tenantInitializer)
        {
            this.tenantInitializer = tenantInitializer;
        }

        public TenantId CreateDemonstrationTenant(string name, string description, bool isDefault)
        {
            lock (syncLock)
            {
                Logger.Info($"Creating demonstration tenant: {name}");
                return CreateTenant(name, description, true, isDefault);
            }
        }

        public TenantId CreateProductionTenant(string name, string description, bool isDefault)
        {
            Logger.Info($"Creating production tenant: {name}");
            lock (syncLock)
            {
                return CreateTenant(name, description, false, isDefault);
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
            tenant.IsInitialized = true;
        }

        public abstract TenantId[] GetTenantIds();

        public abstract Tenant[] GetTenants();

        public bool IsActive(TenantId tenantId)
        {
            if (tenantId.HasValue)
            {
                Tenant tenant = FindTenant(tenantId);

                if (tenant == null)
                {
                    throw new ArgumentException($"Invalid tenant Id [{tenantId.Value}]", nameof(tenantId));
                }

                return tenant.IsActive;
            }

            return true;
        }

        public void EnsureTenantIsInitialized(TenantId tenantId)
        {
            if (initializedTenants.Contains(tenantId.Value))
            {
                return;
            }

            Tenant tenant = FindTenant(tenantId);

            if (tenant == null)
            {
                throw new ArgumentException($"Invalid tenant Id [{tenantId.Value}]", nameof(tenantId));
            }

            InitializeTenant(tenant);
            SaveTenant(tenant);
            initializedTenants.Add(tenantId.Value);
        }

        public abstract Tenant FindTenant(TenantId tenantId);

        private TenantId CreateTenant([NotNull] string name, string description, bool isDemo, bool isDefault)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

            if (GetTenants().Any(t => t.Name != null && t.Name.ToLowerInvariant() == name.ToLowerInvariant()))
            {
                throw new ArgumentException($"There is already a tenant named {name}");
            }

            Tenant tenant = new Tenant(name, description, isDemo) { IsActive = true, IsDefault = isDefault };
            SaveTenant(tenant);
            return new TenantId(tenant.Id);
        }

        protected abstract void SaveTenant(Tenant tenant);
    }
}
