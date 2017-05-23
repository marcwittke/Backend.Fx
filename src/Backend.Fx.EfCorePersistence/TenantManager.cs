namespace Backend.Fx.EfCorePersistence
{
    using System;
    using System.Linq;
    using Environment.MultiTenancy;
    using JetBrains.Annotations;
    using Logging;
    using Microsoft.EntityFrameworkCore;

    public class TenantManager<TDbContext> : ITenantManager where TDbContext : DbContext
    {
        private static readonly ILogger Logger = LogManager.Create<TenantManager<TDbContext>>();
        private readonly object syncLock = new object();

        private readonly ITenantInitializer tenantInitializer;
        private readonly Func<TDbContext> dbContextFactory;

        public TenantManager(ITenantInitializer tenantInitializer, Func<TDbContext> dbContextFactory)
        {
            this.tenantInitializer = tenantInitializer;
            this.dbContextFactory = dbContextFactory;
        }

        public TenantId[] GetTenantIds()
        {
            using (var dbContext = dbContextFactory.Invoke())
            {
                return dbContext.Set<Tenant>().Select(t => new TenantId(t.Id)).ToArray();
            }
        }

        public Tenant[] GetTenants()
        {
            using (var dbContext = dbContextFactory.Invoke())
            {
                return dbContext.Set<Tenant>().ToArray();
            }
        }

        public bool IsActive(TenantId tenantId)
        {
            if (!tenantId.HasValue)
            {
                // the null tenant cannot be inactive. But it isn't useful anyway. 
                // this case might occur during unauthenticated calls
                return true;
            }

            using (var dbContext = dbContextFactory.Invoke())
            {
                return dbContext.Set<Tenant>().Find(tenantId.Value).IsActive;
            }
        }

        public TenantId CreateDemonstrationTenant(string name, string description)
        {
            lock (syncLock)
            {
                return CreateTenant(name, description, true);
            }
        }

        public TenantId CreateProductionTenant(string name, string description)
        {
            lock (syncLock)
            {
                return CreateTenant(name, description, false);
            }
        }

        public void EnsureTenantIsInitialized(TenantId tenantId)
        {
            Tenant tenant;
            using (var dbContext = dbContextFactory.Invoke())
            {
                tenant = dbContext.Set<Tenant>().Find(tenantId.Value);
            }

            if (tenant == null)
            {
                throw new ArgumentException($"Invalid tenant Id [{tenantId.Value}]", nameof(tenantId));
            }

            InitializeTenant(tenant);
        }

        private TenantId CreateTenant([NotNull] string name, string description, bool isDemo)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            using (var dbContext = dbContextFactory.Invoke())
            {
                if (dbContext.Set<Tenant>().Any(t => t.Name != null && t.Name.ToLowerInvariant() == name.ToLowerInvariant()))
                {
                    throw new ArgumentException($"There is already a tenant named {name}");
                }
            }

            Tenant tenant = new Tenant(name, description, isDemo) { IsActive = true };
            using (var dbContext = dbContextFactory.Invoke())
            {
                dbContext.Add(tenant);
                dbContext.SaveChanges();
            }

            InitializeTenant(tenant);

            return new TenantId(tenant.Id);
        }

        private void InitializeTenant(Tenant tenant)
        {
            if (tenant.IsInitialized) return;

            var tenantId = new TenantId(tenant.Id);
            tenantInitializer.RunProductiveInitialDataGenerators(tenantId);
            if (tenant.IsDemoTenant)
            {
                tenantInitializer.RunDemoInitialDataGenerators(tenantId);
            }

            using (var dbContext = dbContextFactory.Invoke())
            {
                dbContext.Set<Tenant>().Find(tenantId.Value).IsInitialized = true;
                dbContext.SaveChanges();
            }
        }
    }
}

