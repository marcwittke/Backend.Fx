namespace Backend.Fx.EfCorePersistence
{
    using System;
    using System.Linq;
    using Environment.MultiTenancy;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;

    public class TenantManager<TDbContext> : TenantManager where TDbContext : DbContext
    {
        private readonly Func<TDbContext> dbContextFactory;

        public TenantManager(ITenantInitializer tenantInitializer, Func<TDbContext> dbContextFactory) : base(tenantInitializer)
        {
            this.dbContextFactory = dbContextFactory;
        }

        public override TenantId[] GetTenantIds()
        {
            using (var dbContext = dbContextFactory.Invoke())
            {
                return dbContext.Set<Tenant>().Select(t => new TenantId(t.Id)).ToArray();
            }
        }

        public override Tenant[] GetTenants()
        {
            using (var dbContext = dbContextFactory.Invoke())
            {
                return dbContext.Set<Tenant>().ToArray();
            }
        }
        
        protected override Tenant FindTenant(TenantId tenantId)
        {
            using (var dbContext = dbContextFactory.Invoke())
            {
                return dbContext.Set<Tenant>().Find(tenantId.Value);
            }
        }

        protected override void SaveTenant(Tenant tenant)
        {
            using (var dbContext = dbContextFactory.Invoke())
            {
                var existingTenant = dbContext.Set<Tenant>().Find(tenant.Id);
                if (existingTenant == null)
                {
                    dbContext.Add(tenant);
                }
                else
                {
                    EntityEntry<Tenant> entry = dbContext.Attach(tenant);
                    entry.State = EntityState.Modified;
                }
                dbContext.SaveChanges();
            }
        }
    }
}

